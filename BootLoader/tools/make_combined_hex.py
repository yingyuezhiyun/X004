#!/usr/bin/env python3
"""
make_combined_hex.py
- 合并 bootloader 和 app1，并在 cfg 地址生成 mem_cfg（包含 fw headers）
- 产生 Intel HEX 输出文件

默认 mem_cfg 布局（小端）（若项目实际布局不同，请按注释修改）:
struct fw_header_t {
    uint32_t magic;
    uint32_t fw_size;
    uint32_t fw_crc;
    // optional reserved: uint32_t reserved;
};
struct mem_cfg_t {
    uint32_t bootloader_address;
    uint32_t app1_address;
    uint32_t app2_address;
    uint32_t cfg_address;
    uint32_t active_app;
    uint32_t pending_state;
    fw_header_t app1_hdr;
    fw_header_t app2_hdr;
    // total size here = 6*4 + 2*3*4 = 48 bytes
}

注意：脚本默认 FW_MAGIC/PSTATE 值可通过命令行覆盖。
"""
from __future__ import annotations
import argparse, struct, os, sys, re, json

POLY = 0x04C11DB7

# Replace simple byte-wise CRC with version that matches crc32_mpeg2_bytes_as_le_words:
def crc32_mpeg2_bytes_as_le_words(data: bytes) -> int:
    # Pack data into little-endian 32-bit words (pad with 0), then process each word's bytes MSB-first
    crc = 0xFFFFFFFF
    if len(data) == 0:
        return 0
    words = (len(data) + 3) // 4
    for wi in range(words):
        base = wi * 4
        w = 0
        # b0 is LSB
        for b in range(4):
            idx = base + b
            v = data[idx] if idx < len(data) else 0
            w |= (v & 0xFF) << (8 * b)
        # process bytes in MSB-first order: byte 3,2,1,0
        for byte_i in range(3, -1, -1):
            cur = (w >> (8 * byte_i)) & 0xFF
            crc ^= (cur << 24) & 0xFFFFFFFF
            for _ in range(8):
                if (crc & 0x80000000):
                    crc = ((crc << 1) ^ POLY) & 0xFFFFFFFF
                else:
                    crc = (crc << 1) & 0xFFFFFFFF
    return crc & 0xFFFFFFFF

# Intel HEX parser: returns dict[address] = byte
def read_intel_hex(path: str) -> dict:
    mem = {}
    upper = 0
    with open(path, "r") as f:
        for lineno, line in enumerate(f, 1):
            line = line.strip()
            if not line:
                continue
            if not line.startswith(":"):
                continue
            try:
                rec = bytes.fromhex(line[1:])
            except Exception:
                raise ValueError(f"Invalid HEX on line {lineno}")
            rec_len = rec[0]
            rec_off = (rec[1] << 8) | rec[2]
            rec_type = rec[3]
            data = rec[4:4+rec_len]
            # checksum check optional (skip for brevity)
            if rec_type == 0x00:
                addr = (upper << 16) | rec_off
                for i, b in enumerate(data):
                    mem[addr + i] = b
            elif rec_type == 0x01:
                break
            elif rec_type == 0x04:
                # Extended linear address
                upper = (data[0] << 8) | data[1]
            else:
                # ignore other record types
                pass
    return mem

# Helper: extract contiguous bytes starting at base from mem map
def extract_contiguous_from_map(memmap: dict, base: int) -> bytes:
    if base not in memmap:
        return b""
    out = bytearray()
    addr = base
    while True:
        if addr in memmap:
            out.append(memmap[addr])
            addr += 1
        else:
            break
    return bytes(out)

# Intel HEX helpers
def _checksum(bytes_seq: bytes) -> int:
    s = sum(bytes_seq) & 0xFF
    return ((~s + 1) & 0xFF)

def intel_hex_records_for_region(base_addr: int, data: bytes, record_bytes=16):
    """
    Generate intel hex lines (strings) for given base_addr and data.
    Handles Extended Linear Address records (04) for >64k boundaries.
    """
    lines = []
    upper = None
    offs = 0
    while offs < len(data):
        addr = base_addr + offs
        new_upper = (addr >> 16) & 0xFFFF
        rec_off = addr & 0xFFFF
        if upper != new_upper:
            # emit extended linear address
            upper = new_upper
            rec = bytes([0x02, 0x00, 0x00, 0x04, (upper >> 8) & 0xFF, upper & 0xFF])
            c = _checksum(rec)
            lines.append(":{:02X}{:04X}{:02X}{}{:02X}".format(0x02, 0x0000, 0x04, "{:04X}".format(upper), c).replace("{:04X}".format(upper), "{:04X}".format(upper)))
            # The above formatting ensures correct hex string; simpler alternative below:
            #lines.append(":02000004{:04X}{:02X}".format(upper, (_checksum(bytes([0x02,0x00,0x00,0x04, (upper>>8)&0xFF, upper&0xFF])))))
        chunk = data[offs:offs+record_bytes]
        rec_len = len(chunk)
        rec_hdr = bytes([rec_len, (rec_off >> 8) & 0xFF, rec_off & 0xFF, 0x00])
        rec = rec_hdr + chunk
        c = _checksum(rec)
        lines.append(":" + rec.hex().upper() + "{:02X}".format(c))
        offs += rec_len
    return lines

def _make_record_bytes(offset: int, rectype: int, data: bytes) -> str:
    # offset: 16-bit offset in record header
    hdr = bytes([len(data), (offset >> 8) & 0xFF, offset & 0xFF, rectype])
    rec = hdr + data
    # checksum: two's complement of sum of bytes
    s = sum(rec) & 0xFF
    chksum = ((~s + 1) & 0xFF)
    return ":" + rec.hex().upper() + "{:02X}".format(chksum)

def intel_hex_write(output_path: str, regions: list[tuple[int, bytes]], start_linear: int | None = None, trailing_ela: bool = False):
    """
    regions: list of (base_addr, bytes)
    Writes one Intel HEX file.
    Emits Extended Linear Address (04) only when upper 16 bits change.
    Optionally emits Start Linear Address record (05) before EOF if start_linear provided.
    If trailing_ela=True, emit an ELA record after each region (value = upper of end addr).
    """
    lines = []
    # sort by address
    current_upper = None
    for base, data in sorted(regions, key=lambda x: x[0]):
        offs = 0
        while offs < len(data):
            addr = base + offs
            upper = (addr >> 16) & 0xFFFF
            rec_off = addr & 0xFFFF
            # emit extended linear address record when upper changes
            if current_upper != upper:
                ela_data = bytes([ (upper >> 8) & 0xFF, upper & 0xFF ])
                hdr = bytes([0x02, 0x00, 0x00, 0x04]) + ela_data
                s = sum(hdr) & 0xFF
                chksum = ((~s + 1) & 0xFF)
                lines.append(":" + hdr.hex().upper() + "{:02X}".format(chksum))
                current_upper = upper
            # amount to write without crossing 64k boundary and <=16 bytes
            max_len = min(16, len(data) - offs, 0x10000 - rec_off)
            chunk = data[offs:offs+max_len]
            # build data record (type 00) with offset rec_off
            rec = bytes([len(chunk), (rec_off >> 8) & 0xFF, rec_off & 0xFF, 0x00]) + chunk
            s = sum(rec) & 0xFF
            chksum = ((~s + 1) & 0xFF)
            lines.append(":" + rec.hex().upper() + "{:02X}".format(chksum))
            offs += max_len
        # optional trailing ELA after this region
        if trailing_ela:
            end_addr = base + len(data)
            end_upper = (end_addr >> 16) & 0xFFFF
            ela_data = bytes([ (end_upper >> 8) & 0xFF, end_upper & 0xFF ])
            hdr = bytes([0x02, 0x00, 0x00, 0x04]) + ela_data
            s = sum(hdr) & 0xFF
            chksum = ((~s + 1) & 0xFF)
            lines.append(":" + hdr.hex().upper() + "{:02X}".format(chksum))
            current_upper = end_upper
    # Optionally emit Start Linear Address Record (type 05) with 4-byte BE address
    if start_linear is not None:
        data = bytes([ (start_linear >> 24) & 0xFF, (start_linear >> 16) & 0xFF,
                       (start_linear >> 8) & 0xFF, start_linear & 0xFF ])
        rec = bytes([0x04, 0x00, 0x00, 0x05]) + data
        s = sum(rec) & 0xFF
        chksum = ((~s + 1) & 0xFF)
        lines.append(":" + rec.hex().upper() + "{:02X}".format(chksum))
    # EOF
    lines.append(":00000001FF")
    with open(output_path, "w") as f:
        f.write("\n".join(lines))
    print("Wrote Intel HEX to", output_path)

def le32(x): return struct.pack("<I", x)

def make_mem_cfg_bytes(bl_addr, app1_addr, app2_addr, cfg_addr, active_app, pending_state, app1_hdr, app2_hdr):
    # app*_hdr: tuples (magic, fw_size, fw_crc)
    b = bytearray()
    b += le32(bl_addr)
    b += le32(app1_addr)
    b += le32(app2_addr)
    b += le32(cfg_addr)
    b += le32(active_app)
    b += le32(pending_state)
    # app1 header
    b += le32(app1_hdr[0])
    b += le32(app1_hdr[1])
    b += le32(app1_hdr[2])
    # app2 header
    b += le32(app2_hdr[0])
    b += le32(app2_hdr[1])
    b += le32(app2_hdr[2])
    return bytes(b)

def build_mem_cfg_from_layout(layout: dict, values: dict) -> bytes:
    """
    layout: dict with keys:
      - fields: list of {name, offset, type} where type currently supports 'u32' or 'bytes' with length
      - total_size (optional)
    values: dict mapping field name -> integer or bytes
    Returns bytes of length total_size (or computed from largest field+size)
    """
    fields = layout.get("fields", [])
    # determine total size
    total = layout.get("total_size", 0)
    for f in fields:
        off = f["offset"]
        t = f["type"]
        if t == "u32":
            size = 4
        elif t.startswith("bytes"):
            # allow "bytes:len" or {"type":"bytes","len":N}
            size = f.get("len", int(t.split(":")[1]) if ":" in t else 1)
        else:
            raise ValueError("Unsupported field type: " + t)
        total = max(total, off + size)
    buf = bytearray([0x00] * total)
    for f in fields:
        name = f["name"]
        off = f["offset"]
        t = f["type"]
        if t == "u32":
            v = values.get(name, 0)
            buf[off:off+4] = struct.pack("<I", int(v) & 0xFFFFFFFF)
        elif t.startswith("bytes"):
            size = f.get("len", int(t.split(":")[1]) if ":" in t else 1)
            v = values.get(name, b"\x00" * size)
            if isinstance(v, str):
                v = bytes.fromhex(v)
            if len(v) < size:
                v = v + b"\x00" * (size - len(v))
            buf[off:off+size] = v[:size]
        else:
            raise ValueError("Unsupported field type: " + t)
    return bytes(buf)

def parse_mem_cfg_from_map_with_layout(memmap: dict, cfg_addr: int, layout: dict) -> dict:
    """
    Read fields from memmap using layout and return dict mapping field name -> value (int or bytes)
    """
    out = {}
    for f in layout.get("fields", []):
        name = f["name"]
        off = f["offset"]
        t = f["type"]
        if t == "u32":
            # read 4 bytes little-endian, missing bytes -> 0
            b = bytes([ memmap.get(cfg_addr + off + i, 0) for i in range(4) ])
            out[name] = struct.unpack("<I", b)[0]
        elif t.startswith("bytes"):
            size = f.get("len", int(t.split(":")[1]) if ":" in t else 1)
            b = bytes([ memmap.get(cfg_addr + off + i, 0) for i in range(size) ])
            out[name] = b
        else:
            raise ValueError("Unsupported field type: " + t)
    return out

# 默认地址与大小（与 Core/Inc/global_cfg.h 中定义的布局保持一致）
# BL = 0x08000000, APP1 = BL + 0x20000, APP2 = BL + 0x80000, CFG = BL + 0xE0000
DEFAULT_BL_ADDR   = 0x08000000
DEFAULT_APP1_ADDR = 0x08020000
DEFAULT_CFG_ADDR  = 0x080E0000
DEFAULT_CFG_SIZE  = 0

# 默认 cfg 布局（对应 Core/Inc/global_cfg.h 中的 mem_cfg_t）
# offsets are in bytes from cfg base, little-endian u32 fields
DEFAULT_CFG_LAYOUT = {
    "fields": [
        {"name": "bootloader_address", "offset": 0, "type": "u32"},
        {"name": "app_address",       "offset": 4, "type": "u32"},
        {"name": "cfg_address",       "offset": 8, "type": "u32"},
        {"name": "pending_state",     "offset": 12, "type": "u32"},
        {"name": "app_hdr_magic",     "offset": 16, "type": "u32"},
        {"name": "app_hdr_fw_size",   "offset": 20, "type": "u32"},
        {"name": "app_hdr_fw_crc",    "offset": 24, "type": "u32"}
    ],
    "total_size": 28
}

def parse_mem_cfg_from_map(memmap: dict, cfg_addr: int):
    # read 48 bytes (6 u32 + 2*(3 u32)) from map, missing bytes treated as 0
    def read_u32(addr):
        return struct.unpack("<I", bytes([memmap.get(addr+i,0) for i in range(4)]))[0]
    boot = read_u32(cfg_addr + 0)
    app1 = read_u32(cfg_addr + 4)
    app2 = read_u32(cfg_addr + 8)
    cfg = read_u32(cfg_addr + 12)
    active = read_u32(cfg_addr + 16)
    pending = read_u32(cfg_addr + 20)
    app1_magic = read_u32(cfg_addr + 24)
    app1_size = read_u32(cfg_addr + 28)
    app1_crc = read_u32(cfg_addr + 32)
    app2_magic = read_u32(cfg_addr + 36)
    app2_size = read_u32(cfg_addr + 40)
    app2_crc = read_u32(cfg_addr + 44)
    return {
        "boot": boot, "app1": app1, "app2": app2, "cfg": cfg,
        "active": active, "pending": pending,
        "app1_magic": app1_magic, "app1_size": app1_size, "app1_crc": app1_crc,
        "app2_magic": app2_magic, "app2_size": app2_size, "app2_crc": app2_crc
    }

def find_start_linear_in_hex(path: str) -> int | None:
    """扫描 Intel HEX 文件，寻找第一个 type 05 记录并返回大端解析的地址，找不到返回 None"""
    try:
        with open(path, "r") as f:
            for line in f:
                line = line.strip()
                if not line or not line.startswith(":"):
                    continue
                rec = bytes.fromhex(line[1:])
                rec_len = rec[0]
                rec_type = rec[3]
                if rec_type == 0x05 and rec_len == 4:
                    data = rec[4:8]
                    # data is big-endian 4 bytes
                    addr = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]
                    return addr
    except Exception:
        pass
    return None

def main():
    p = argparse.ArgumentParser(description="Merge bootloader + app1 into one Intel HEX and generate cfg area.")
    p.add_argument("--boot", required=True, help="bootloader hex/bin path (hex supported)")
    p.add_argument("--app1", required=True, help="app1 hex/bin path (hex supported)")
    p.add_argument("--out", required=True, help="output intel hex path")
    p.add_argument("--bl-addr", type=lambda x: int(x,0), default=DEFAULT_BL_ADDR, help=f"bootloader load address (default: 0x{DEFAULT_BL_ADDR:08X})")
    p.add_argument("--app1-addr", type=lambda x: int(x,0), default=DEFAULT_APP1_ADDR, help=f"app1 load address (default: 0x{DEFAULT_APP1_ADDR:08X})")
    p.add_argument("--cfg-addr", type=lambda x: int(x,0), default=DEFAULT_CFG_ADDR, help=f"cfg area base address (default: 0x{DEFAULT_CFG_ADDR:08X})")
    p.add_argument("--app2-addr", type=lambda x: int(x,0), default=0xFFFFFFFF, help="app2 address (optional)")
    p.add_argument("--active-app", type=lambda x: int(x,0), default=None, help="active app address (default = app1-addr)")
    p.add_argument("--pending-state", type=lambda x: int(x,0), default=0, help="pending_state numeric value (default 0)")
    p.add_argument("--fw-magic", type=lambda x: int(x,0), default=0xA5A5A5A5, help="FW_MAGIC value to write into header (default: 0xA5A5A5A5)")
    p.add_argument("--pad-byte", type=lambda x: int(x,0), default=0xFF, help="fill byte for gaps")
    p.add_argument("--cfg-size", type=int, default=DEFAULT_CFG_SIZE, help="minimum cfg blob size in bytes to write at cfg-addr (used to pad/truncate the mem_cfg blob)")
    p.add_argument("--verify", action="store_true", help="After writing output hex, read it and verify/show cfg fields")
    p.add_argument("--start-linear-from", choices=["boot","app1","addr"], default="boot",
                   help="Source for Start Linear Address record (05). 'boot' uses bootloader addr (default). 'app1' uses app1 reset vector. 'addr' uses --start-linear-addr.")
    p.add_argument("--start-linear-addr", type=lambda x: int(x,0), default=None,
                   help="Explicit start linear address (used when --start-linear-from=addr).")
    p.add_argument("--trailing-ela", action="store_true", help="Emit an ELA (type 04) record after each region end (stylistic)")
    p.add_argument("--cfg-desc", help="Optional JSON file describing cfg layout. If provided, used to pack/parse mem_cfg. See README for format.")
    args = p.parse_args()

    if not os.path.isfile(args.boot) or not os.path.isfile(args.app1):
        print("boot or app1 file not found")
        sys.exit(1)

    # Read boot input: support .hex (Intel HEX) or raw binary
    def read_input(path, base_addr=None):
        if re.search(r"\.hex$", path, re.IGNORECASE):
            memmap = read_intel_hex(path)
            return ("map", memmap)
        else:
            with open(path, "rb") as f:
                return ("bin", f.read())

    boot_type, boot_data = read_input(args.boot)
    app1_type, app1_data = read_input(args.app1)

    # Prepare boot bytes and app1 bytes for regions
    regions = []

    # Boot: if hex, convert map segment into contiguous region(s) starting from min addr to max addr present
    if boot_type == "map":
        # find min and max present addresses for boot file (we will add whole contiguous block starting at min)
        addrs = sorted(boot_data.keys())
        if not addrs:
            print("boot hex contains no data")
            sys.exit(1)
        boot_base = addrs[0]
        boot_bytes = bytearray()
        # build contiguous chunk from base to last continuous address
        cur = boot_base
        while cur in boot_data:
            boot_bytes.append(boot_data[cur])
            cur += 1
        regions.append((boot_base, bytes(boot_bytes)))
    else:
        # binary: place at provided bl-addr
        regions.append((args.bl_addr, boot_data))

    # App1: if hex, extract contiguous starting at provided app1-addr
    if app1_type == "map":
        memmap = app1_data
        app1_blob = extract_contiguous_from_map(memmap, args.app1_addr)
        if len(app1_blob) == 0:
            print("app1 hex has no data at specified app1-addr 0x{:08X}".format(args.app1_addr))
            sys.exit(1)
        regions.append((args.app1_addr, app1_blob))
        app1_size = len(app1_blob)
        app1_crc = crc32_mpeg2_bytes_as_le_words(app1_blob)
    else:
        # binary file
        app1_blob = app1_data
        regions.append((args.app1_addr, app1_blob))
        app1_size = len(app1_blob)
        app1_crc = crc32_mpeg2_bytes_as_le_words(app1_blob)

    print("App1: size =", hex(app1_size), "crc =", hex(app1_crc))

    # 决定 Start Linear Address (type 05)
    start_linear = args.bl_addr  # 默认指向 bootloader

    if args.start_linear_from == "boot":
        # 如果 boot 输入的是 hex，优先使用 boot.hex 中的 type 05（若存在）
        try:
            if boot_type == "map":
                bl_start = find_start_linear_in_hex(args.boot)
                if bl_start is not None:
                    start_linear = bl_start
                    print(f"Using Start Linear Address from boot.hex: 0x{start_linear:08X}")
                else:
                    print(f"No type 05 in boot.hex; using bootloader addr 0x{start_linear:08X}")
            else:
                print(f"Boot input is binary; using bootloader addr 0x{start_linear:08X}")
        except Exception as e:
            print("Error reading start-linear from boot input, fallback to boot addr:", e)
            start_linear = args.bl_addr

    elif args.start_linear_from == "app1":
        # 保持原有逻辑：从 app1 二进制读取 reset vector
        try:
            if app1_size >= 8:
                reset_vec = struct.unpack_from("<I", app1_blob, 4)[0]
                start_linear = reset_vec
                print(f"Using app1 reset vector as start linear address: 0x{start_linear:08X}")
            else:
                print(f"app1 too small to read reset vector, falling back to bootloader 0x{start_linear:08X}")
        except Exception as e:
            print("Failed to read app1 reset vector, falling back to bootloader address:", e)
            start_linear = args.bl_addr

    elif args.start_linear_from == "addr":
        if args.start_linear_addr is not None:
            start_linear = args.start_linear_addr
            print(f"Using explicit start linear address: 0x{start_linear:08X}")
        else:
            print("Warning: --start-linear-from=addr but --start-linear-addr not provided; falling back to bootloader address 0x{:08X}".format(start_linear))

    app2_addr = args.app2_addr
    active_app = args.active_app if args.active_app is not None else args.app1_addr

    # prepare values for mem_cfg fields (legacy names and default app2 header)
    app1_hdr = {"magic": args.fw_magic, "fw_size": app1_size, "fw_crc": app1_crc}
    app2_hdr = {"magic": 0xFFFFFFFF, "fw_size": 0, "fw_crc": 0xFFFFFFFF}

    # determine layout to use
    if args.cfg_desc:
        try:
            with open(args.cfg_desc, "r") as jf:
                cfg_layout = json.load(jf)
        except Exception as e:
            print("Failed to read cfg-desc JSON:", e)
            sys.exit(1)
    else:
        cfg_layout = DEFAULT_CFG_LAYOUT

    # build values dict mapping layout field names to values
    values = {}
    # fill common names expected by layout (legacy and typical)
    values["bootloader_address"] = args.bl_addr
    values["app1_address"] = args.app1_addr
    values["app2_address"] = app2_addr
    values["cfg_address"] = args.cfg_addr
    values["active_app"] = active_app
    values["pending_state"] = args.pending_state
    # map app1/app2 header names for default layout
    # default layout uses app1_magic/size/crc etc.
    values["app1_magic"] = app1_hdr["magic"]
    values["app1_size"] = app1_hdr["fw_size"]
    values["app1_crc"] = app1_hdr["fw_crc"]
    values["app2_magic"] = app2_hdr["magic"]
    values["app2_size"] = app2_hdr["fw_size"]
    values["app2_crc"] = app2_hdr["fw_crc"]
    # Also provide names matching Core/Inc/global_cfg.h mem_cfg_t (single app_hdr)
    values["app_address"] = args.app1_addr
    values["app_hdr_magic"] = app1_hdr["magic"]
    values["app_hdr_fw_size"] = app1_hdr["fw_size"]
    values["app_hdr_fw_crc"] = app1_hdr["fw_crc"]

    mem_cfg_blob = build_mem_cfg_from_layout(cfg_layout, values)
    # cfg-size 说明输出：按用户请求大小与实际mem_cfg大小取最大值，然后按32字节对齐
    # 但实际存储在 Flash 中时，固件使用一个 Flash_info 头(2 bytes header, 2 bytes datasize)
    # 放在每个 32-byte 区块的起始位置，随后紧跟 mem_cfg 数据。因此这里需要构造
    # head + mem_cfg 并按 32 字节对齐后写入 args.cfg_addr。
    min_needed = len(mem_cfg_blob)
    requested = args.cfg_size
    # If requested==0 (default), do not enforce larger minimum; only pad to 32-byte multiple
    final_size = min_needed if requested == 0 else max(min_needed, requested)
    # 我们不再对 mem_cfg 本体提前扩展到 32 字节的倍数。
    # 行为：mem_cfg 本体长度为实际所需 (或显式请求的最小值)，head->datasize 使用实际 mem_cfg 长度，
    # 然后把 head + mem_cfg 这个整体向上填充到 32 字节的倍数再写入 flash（固件读取时按 datasize 读取）。
    if requested == 0:
        final_size = min_needed
        print(f"CFG: preparing mem_cfg (body) at 0x{args.cfg_addr:08X}, mem_cfg={min_needed} bytes -> will pad outer record to 32-byte multiple")
    else:
        final_size = max(min_needed, requested)
        print(f"CFG: preparing mem_cfg (body) at 0x{args.cfg_addr:08X}, requested cfg-size={requested} bytes, mem_cfg={min_needed} bytes -> body size={final_size} bytes")

    # if requested > 0 and requires mem_cfg to be larger, pad mem_cfg_body to final_size, otherwise keep raw
    if final_size > min_needed:
        mem_cfg_body = mem_cfg_blob + bytes([0x00]) * (final_size - min_needed)
    else:
        mem_cfg_body = mem_cfg_blob

    FLASH_PRAGMA_HEAD = 0xAAAA
    head = struct.pack("<HH", FLASH_PRAGMA_HEAD, len(mem_cfg_body))
    flash_blob = head + mem_cfg_body
    # pad the whole flash_blob up to 32-byte multiple
    pad_len = ((len(flash_blob) + 31) // 32) * 32 - len(flash_blob)
    if pad_len:
        flash_blob = flash_blob + bytes([0x00]) * pad_len

    print(f"CFG: writing flash record at 0x{args.cfg_addr:08X}, total {len(flash_blob)} bytes (incl header, padded to 32)")
    regions.append((args.cfg_addr, flash_blob))

    # write hex: 传入计算得到的 start_linear
    intel_hex_write(args.out, regions, start_linear=start_linear, trailing_ela=args.trailing_ela)
    print("Done. app1_crc=0x{:08X}".format(app1_crc))

    # verification / display if requested
    if args.verify:
        print("Verifying CFG in output hex:", args.out)
        try:
            memmap = read_intel_hex(args.out)
        except Exception as e:
            print("Failed to read generated hex:", e)
            sys.exit(1)
        # firmware stores mem_cfg after a 4-byte Flash_info header, so parse from cfg_addr+4
        parsed = parse_mem_cfg_from_map_with_layout(memmap, args.cfg_addr + 4, cfg_layout)
        print("Parsed mem_cfg at 0x{:08X}:".format(args.cfg_addr))
        for f in cfg_layout.get("fields", []):
            name = f["name"]
            val = parsed.get(name)
            if isinstance(val, int):
                print(f"  {name:20s}: 0x{val:08X}")
            else:
                # bytes
                print(f"  {name:20s}: {val.hex()}")

        # basic comparison for common fields if present
        ok = True
        if "bootloader_address" in parsed and parsed["bootloader_address"] != args.bl_addr:
            print("  MISMATCH bootloader_address (expected 0x{:08X})".format(args.bl_addr)); ok = False
        if "app1_address" in parsed and parsed["app1_address"] != args.app1_addr:
            print("  MISMATCH app1_address (expected 0x{:08X})".format(args.app1_addr)); ok = False
        if "cfg_address" in parsed and parsed["cfg_address"] != args.cfg_addr:
            print("  MISMATCH cfg_address (expected 0x{:08X})".format(args.cfg_addr)); ok = False
        # check app1 header fields if present
        if parsed.get("app1_magic") is not None and parsed.get("app1_magic") != args.fw_magic:
            print("  MISMATCH app1_magic (expected 0x{:08X})".format(args.fw_magic)); ok = False
        if parsed.get("app1_size") is not None and parsed.get("app1_size") != app1_size:
            print("  MISMATCH app1_size (expected 0x{:08X})".format(app1_size)); ok = False
        if parsed.get("app1_crc") is not None and parsed.get("app1_crc") != app1_crc:
            print("  MISMATCH app1_crc (expected 0x{:08X})".format(app1_crc)); ok = False
        if ok:
            print("CFG verification: OK")
        else:
            print("CFG verification: MISMATCH detected (see above)")

if __name__ == "__main__":
    main()
