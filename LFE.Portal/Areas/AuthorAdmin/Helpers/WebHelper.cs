using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;

namespace LFE.Portal.Areas.AuthorAdmin.Helpers
{
    public class WebHelper
    {
        public static ReportEnums.ePeriodSelectionKinds DEFAULT_PERIOD_SELECTION = ReportEnums.ePeriodSelectionKinds.week;
        
        public WebHelper()
        {
            AuthorServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();
        }
        public IAuthorAdminServices AuthorServices { get; private set; }

        public static int[] PageSizes
        {
            get { return new[] { 10, 15, 20 , 50 }; }
        }             
    }
}