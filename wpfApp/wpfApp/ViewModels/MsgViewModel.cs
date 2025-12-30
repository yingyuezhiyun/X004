using MaterialDesignThemes.Wpf;
using wpfApp.Common;
using wpfApp.Common.Dialog;
using Prism.Commands;
using Prism.Mvvm;
//using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Dialogs;
using System.Windows;

namespace wpfApp.ViewModels
{

   public class MsgViewModel : BindableBase, IDialogHostAware
    {
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        public MsgViewModel()
        {
            //Button1Command = new DelegateCommand(Save);
            //Button2Command = new DelegateCommand(Cancel);
            //Button3Command = new DelegateCommand(Ignore);
            //ExecuteCommand = new DelegateCommand<string>(Execute);
        }


        private MsgType msgType = MsgType.YesNo ;

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged(); }
        }

        private List<ButtonPara> buttonParas;

        public List<ButtonPara> ButtonParas
        {
            get { return buttonParas; }
            set { buttonParas = value; RaisePropertyChanged(); }
        }
      

        public class ButtonPara : BindableBase
        {

            private string text;
            public string Text
            {
                get { return text; }
                set { text = value; RaisePropertyChanged(); }
            }
            private Visibility visibility;
            public Visibility Visibility
            {
                get { return visibility; }
                set { visibility = value; RaisePropertyChanged(); }
            }
            public DelegateCommand Command { get; set; }

        }


        //private void Execute(string obj)
        //{
        //    switch (obj)
        //    {
        //        case "Button1Comand": break;
        //        case "Button2Comand": break;
        //        case "Button3Comand": break;
        //        default: break;
        //    }
        //}
        private void CancelCommand()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        private void YesCommand()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {

                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Yes) { Parameters = new DialogParameters() });     
                
            }
        }

        private void IgnoreCommand()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {

                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Ignore) { Parameters = new DialogParameters() });
              
            }
        }
        private void RetryCommand()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {

                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Retry) { Parameters = new DialogParameters() });

            }
        }

        public string DialogHostName { get; set; } = "Root";
        public DelegateCommand Button1Command { get; set; }
        public DelegateCommand Button2Command { get; set; }

        public DelegateCommand Button3Command { get; set; }

        
        void CreateButtonsCommand()
        {
            ButtonParas = new List<ButtonPara>();
            for (int i = 0; i < 3; i++)
            {
                ButtonParas.Add(new ButtonPara());
            }
            switch (msgType)
            {
                case MsgType.None:
                    for (int i = 0; i < 3; i++)
                    {
                        ButtonParas[i].Visibility = Visibility.Collapsed;
                    }
                    break;
                case MsgType.Yes:
                    ButtonParas[0].Visibility = Visibility.Visible;
                    ButtonParas[0].Command = new DelegateCommand(YesCommand);
                    ButtonParas[0].Text = "确定";
                    ButtonParas[1].Visibility = Visibility.Collapsed;
                    ButtonParas[2].Visibility = Visibility.Collapsed;

                    break;
                case MsgType.YesNo:
                    ButtonParas[0].Visibility = Visibility.Visible;
                    ButtonParas[0].Command = new DelegateCommand(YesCommand);
                    ButtonParas[0].Text = "确定";
                    ButtonParas[1].Visibility = Visibility.Visible;
                    ButtonParas[1].Command = new DelegateCommand(CancelCommand);
                    ButtonParas[1].Text = "取消";
                    ButtonParas[2].Visibility = Visibility.Collapsed;
                    break;
                case MsgType.YesNoCancel:
                    break;
                case MsgType.YesIgnoreCancel:
                    break;
                case MsgType.YesIgnoreRetry:
                    ButtonParas[0].Visibility = Visibility.Visible;
                    ButtonParas[0].Command = new DelegateCommand(YesCommand);
                    ButtonParas[0].Text = "确定";
                    ButtonParas[1].Visibility = Visibility.Visible;
                    ButtonParas[1].Command = new DelegateCommand(IgnoreCommand);
                    ButtonParas[1].Text = "忽略";
                    ButtonParas[2].Visibility = Visibility.Visible;
                    ButtonParas[2].Command = new DelegateCommand(RetryCommand);
                    ButtonParas[2].Text = "重试";
                    break;
                case MsgType.YesRetryAbort:
                    break;
                default:
                    break;
            }
        }

        public void OnDialogOpend(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Title"))
                Title = parameters.GetValue<string>("Title");

            if (parameters.ContainsKey("Content"))
                Content = parameters.GetValue<string>("Content");

            if (parameters.ContainsKey("MsgType"))
                msgType = parameters.GetValue<MsgType>("MsgType");
            CreateButtonsCommand();
        }

      
    }
}
