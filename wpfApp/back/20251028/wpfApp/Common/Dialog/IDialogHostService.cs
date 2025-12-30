//using Prism.Services.Dialogs;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.Dialog
{
    public interface IDialogHostService:IDialogService
    {
        Task<IDialogResult> MyShowDialog(string name, IDialogParameters parameters = null, string dialogHostName = "Root");
    }
}
