using System;

namespace ModLoader.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ItemId : Attribute
    {
        public string UniqueValue { get; private set; }
        
        public ItemId(string uniqueValue)
        {
            UniqueValue = uniqueValue;
        }
    }
}