using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfApp.Views
{
    /// <summary>
    /// ProgressView.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressView : UserControl
    {
        public ProgressView()
        {
            InitializeComponent();
        }
        public ProgressView(string Message)
        {
            InitializeComponent();
            if (Message != null)
            {
                TipText.Text = Message;
            }
            else
            {
                TipText.Visibility = Visibility.Collapsed;  
            }
        }
        public string Message
        {
            get => TipText?.Text;
            set
            {
                if (TipText == null) return;
                if (value != null)
                {
                    TipText.Visibility = Visibility.Visible;
                    TipText.Text = value;
                }
                else
                {
                    TipText.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
