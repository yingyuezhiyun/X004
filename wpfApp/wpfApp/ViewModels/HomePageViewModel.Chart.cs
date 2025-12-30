
using wpfApp.Extensions;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using System.Threading;

using Microsoft.Win32;
using System.Globalization;
using MathNet.Numerics;
using ScottPlot.WPF;

namespace wpfApp.ViewModels
{
    public partial class HomePageViewModel
    {

        public WpfPlot scottplot { get; set; }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ManualResetEvent resetEvent = new ManualResetEvent(true);


        /// <summary>
        /// Create a SignalList, add it to the plot, and return it.
        /// A SignalList is a ScatterPlot that is designed to grow using Add() and AddRange() methods.
   

        private void CreateMainChart()
        {
            scottplot = new WpfPlot();
    

        }




        public void ZoomAndVisibility()
        {
    

        }




        public void SaveCofi()
        {
            OperateIniFile.WriteIniData("User", "MainWave.IsVisible", MainWave.IsVisible.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "MainWave.Frequency", MainWave.Frequency.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "MainWave.Maginitude", MainWave.Maginitude.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "MainWave.Phase", MainWave.Phase.ToString(), @"..\..\..\Confi.ini");

            OperateIniFile.WriteIniData("User", "SecondWave.IsEnable", SecondWave.IsEnable.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "SecondWave.IsVisible", SecondWave.IsVisible.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "SecondWave.Frequency", SecondWave.Frequency.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "SecondWave.Maginitude", SecondWave.Maginitude.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "SecondWave.Phase", SecondWave.Phase.ToString(), @"..\..\..\Confi.ini");

            OperateIniFile.WriteIniData("User", "ThirdWave.IsEnable", ThirdWave.IsEnable.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "ThirdWave.IsVisible", ThirdWave.IsVisible.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "ThirdWave.Frequency", ThirdWave.Frequency.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "ThirdWave.Maginitude", ThirdWave.Maginitude.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "ThirdWave.Phase", ThirdWave.Phase.ToString(), @"..\..\..\Confi.ini");

            OperateIniFile.WriteIniData("User", "Noisy.IsEnable", Noisy.IsEnable.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "Noisy.IsVisible", Noisy.IsVisible.ToString(), @"..\..\..\Confi.ini");
            OperateIniFile.WriteIniData("User", "Noisy.StdDev", Noisy.StdDev.ToString(), @"..\..\..\Confi.ini");


        }
        public void ReadConfi()
        {
            string temp;
            bool enable;
            double data;

            temp = OperateIniFile.ReadIniData("User", "WaveLenth", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }

            temp = OperateIniFile.ReadIniData("User", "MaxMaginitude", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "MaginitudeTick", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }
            temp = OperateIniFile.ReadIniData("User", "BaudRate", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
                
            }
            temp = OperateIniFile.ReadIniData("User", "ScaleZoom", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }
            temp = OperateIniFile.ReadIniData("User", "IsScaleZoom", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
              
            }


            temp = OperateIniFile.ReadIniData("User", "MainWave.IsVisible", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "MainWave.Frequency", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "MainWave.Maginitude", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }
            temp = OperateIniFile.ReadIniData("User", "MainWave.Phase", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }


            temp = OperateIniFile.ReadIniData("User", "SecondWave.IsEnable", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "SecondWave.IsVisible", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "SecondWave.Frequency", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }
            temp = OperateIniFile.ReadIniData("User", "SecondWave.Maginitude", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "SecondWave.Phase", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }


            temp = OperateIniFile.ReadIniData("User", "ThirdWave.IsEnable", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "ThirdWave.IsVisible", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
                
            }
            temp = OperateIniFile.ReadIniData("User", "ThirdWave.Frequency", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "ThirdWave.Maginitude", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
              
            }
            temp = OperateIniFile.ReadIniData("User", "ThirdWave.Phase", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
                
            }

            temp = OperateIniFile.ReadIniData("User", "Noisy.IsEnable", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "Noisy.IsVisible", null, @"..\..\..\Confi.ini");
            if (bool.TryParse(temp, out enable))
            {
               
            }
            temp = OperateIniFile.ReadIniData("User", "Noisy.StdDev", null, @"..\..\..\Confi.ini");
            if (double.TryParse(temp, out data))
            {
               
            }


        }


        public void PlotWave()
        {



            SaveCofi();
      

      

      



           
          
        }






    }
}
