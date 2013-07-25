using System;

namespace PieDb
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message){}
    }
}