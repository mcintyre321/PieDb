using System;
using System.Runtime.CompilerServices;

namespace PieDb
{
    public static class PieIdExtension
    {
        static ConditionalWeakTable<object, string> KeyTable = new ConditionalWeakTable<object, string>();

        public static string PieId(this object obj, string id = null)
        {
            return KeyTable.GetValue(obj, key => id ?? Guid.NewGuid().ToString());
        }
    }
}