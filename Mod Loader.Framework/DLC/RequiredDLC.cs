using System;

namespace ModLoader.Framework.DLC
{
    // ReSharper disable once InconsistentNaming
    public class RequiredDLC : Attribute
    {
        // ReSharper disable once InconsistentNaming
        public VTOLVRDLC DLC { get; set; }
        
        public RequiredDLC(VTOLVRDLC requiredDlc)
        {
            DLC = requiredDlc;
        }
    }
}