using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Extensions;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace wpfApp.ViewModels
{
    public partial class PLDMainViewModel
    {
        System.Timers.Timer bitTimer;
        System.Timers.Timer connectTimer;
        private void BitTimerInit()
        {
            int interval = 1000;
            bitTimer = new System.Timers.Timer(interval);
            bitTimer.Enabled = true;
            bitTimer.AutoReset = true;
            bitTimer.Elapsed += (s, e) =>
            {
                DevQueryParam((PLDParams.PLDParamsToQuery.ALL_PARA));
                // DevQueryParam((PLDParams.PLDParamsToQuery.M_LCM));
            };
            bitTimer.Stop();
        }

        bool isIgnoreDevDetect = false;
        async Task ConnectTimeOut()
        {

            for (int i = 0; i < 4; i++)
            {
                SettingParamsToShow.TECParams[i].IsWork = false;
            }
            SettingParamsToShow.LDParams[0].IsWork = false;
            SettingParamsToShow.LDParams[1].IsWork = false;
            MeasureParams.StatusInfo.ConnectStatus.Value = "连接中...";

            var dialogResult = await dialogHostService.Question("警告", "\r\n未检测到设备, 或设备已离线！", msgType: MsgType.YesIgnoreRetry);
            //if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
            if (dialogResult.Result == Prism.Dialogs.ButtonResult.Retry)
            {
                return;
            }
            else if (dialogResult.Result == Prism.Dialogs.ButtonResult.Yes)
            {
                DevClose();
            }
            else if (dialogResult.Result == Prism.Dialogs.ButtonResult.Ignore)
            {
                isIgnoreDevDetect = true;
            }
        }
        void ConnectTimerInit()
        {
            int interval = 5000;
            connectTimer = new System.Timers.Timer(interval);
            connectTimer.Enabled = true;
            connectTimer.AutoReset = true;
            connectTimer.Elapsed += (s, e) =>
            {
                if (!isIgnoreDevDetect)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    { ConnectTimeOut(); });
                }

            };
            connectTimer.Stop();
        }




    }
}
