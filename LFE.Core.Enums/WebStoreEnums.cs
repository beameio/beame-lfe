using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class WebStoreEnums
    {
        [Flags]
        public enum StoreStatus
        {
             [Description("Draft")] Draft           = 1
            ,[Description("Published")] Published   = 2
            ,[Description("Deleted")] Deleted  = 8
            ,[Description("Unknown")] Unknown = 16
        }
    }
}
