using LFE.Core.Enums;

namespace LFE.Portal.Areas.PortalAdmin.Helpers
{
    public class WebHelper
    {
        public static ReportEnums.ePeriodSelectionKinds DEFAULT_PERIOD_SELECTION = ReportEnums.ePeriodSelectionKinds.lastMonth;
        
       
        public static int[] PageSizes
        {
            get { return new[] { 10, 15, 20 , 50 }; }
        }
                           
    }
}