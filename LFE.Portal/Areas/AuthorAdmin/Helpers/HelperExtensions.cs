using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.Portal.Areas.AuthorAdmin.Helpers
{
    public static class HelperExtensions
    {
        public static ReportEnums.ePeriodSelectionKinds ToPeriodSelectionKind(this int periodSelectionKind)
        {
            return Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(periodSelectionKind.ToString());
        }
        public static ReportEnums.ePeriodSelectionKinds ToPeriodSelectionKind(this int? periodSelectionKind)
        {
            return periodSelectionKind != null ? Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(periodSelectionKind.ToString()) : ReportEnums.ePeriodSelectionKinds.all;
        }
    }
}