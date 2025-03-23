using System;

namespace ModLoader.Framework.Exceptions
{
    public class ItemIdMissingException : Exception
    {
        public ItemIdMissingException(string message) : base(message)
        {
            
        }
    }
}