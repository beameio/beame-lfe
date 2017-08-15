using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Portal.Areas.WidgetEndPoint.Models
{
    public class PluginIndexToken : BaseModelState
    {
        public string Uid { get; set; }
        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }
    }
}