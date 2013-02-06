using System;
using System.ComponentModel;
using System.Globalization;

namespace PieDb.Search
{
    public class ReflectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return null;
        }
    }
}