using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AZE.View
{
    public class EverythingConverterValue
    {
        public object ConditionValue { get; set; }

        public object ResultValue { get; set; }
    }

    public class EverythingConverterList : List<EverythingConverterValue>
    {
    }

    public class EverythingConverter : IValueConverter
    {
        public EverythingConverterList Conditions { get; set; } = new EverythingConverterList();

        public object NullResultValue { get; set; }

        public object NullBackValue { get; set; }

        public object NotNullResultValue { get; set; }

        public object NotNullBackValue { get; set; }

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            object result = null;

            if (this.Conditions.Any())
            {
                result = this.Conditions.Where(x => x.ConditionValue.Equals(value)).Select(x => x.ResultValue).FirstOrDefault();
            }

            return result ?? (value == null ? this.NullResultValue : this.NotNullResultValue);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            object result = null;

            if (!this.Conditions.Any())
            {
                result = this.Conditions.Where(x => x.ResultValue.Equals(value)).Select(x => x.ConditionValue).FirstOrDefault();
            }

            return result ?? (value == null ? this.NullBackValue : this.NotNullBackValue);
        }
    }
}