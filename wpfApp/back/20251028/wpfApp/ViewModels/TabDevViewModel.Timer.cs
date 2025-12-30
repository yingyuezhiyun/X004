using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Extensions;

namespace wpfApp.ViewModels
{
    public partial class TabDevViewModel
    {
        System.Timers.Timer queryTimer;
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
                DevGetParam(DevParamsFromGet.BIT);
            };
            bitTimer.Stop();
        }

        private void QueryTimerInit()
        {
            int interval = 500;
            queryTimer = new System.Timers.Timer(interval);
            queryTimer.Enabled = true;
            queryTimer.AutoReset = true;
            queryTimer.Elapsed += (s, e) =>
            {
                DevGetParam(DevParamsFromGet.M_ALL);
            };
            queryTimer.Stop();
        }

        async Task ConnectTimeOut()
        {

            var dialogResult = await dialogHostService.Question("警告", NameSpace + "\r\n\r\n未检测到设备, 或设备已离线！", msgType: MsgType.YesIgnoreRetry);
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

        bool isIgnoreDevDetect = false;
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
