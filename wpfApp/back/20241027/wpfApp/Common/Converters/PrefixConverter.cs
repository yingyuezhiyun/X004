using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static OfficeOpenXml.ExcelErrorValue;

namespace wpfApp.Common.Converters
{
    public class PrefixConverter : IValueConverter
    {
     
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            return $"当前:{value}";
            //return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
