using System.Web.Mvc;
using LFE.Core.Enums;

namespace LFE.Portal.Areas.AuthorAdmin.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString ToCouponKindChecked(this HtmlHelper htmlHelper, CourseEnums.eCouponKinds value, CourseEnums.eCouponKinds selected)
        {
            var attr = value == selected ? " checked " : string.Empty;

            return MvcHtmlString.Create(attr);
        }

        public static MvcHtmlString ToCouponKindReadonly(this HtmlHelper htmlHelper, int couponId)
        {
            var attr = couponId < 0 ? string.Empty : " disabled ";

            return MvcHtmlString.Create(attr);
        }

        public static MvcHtmlString ToStoreCategoryChecked(this HtmlHelper htmlHelper, bool attached)
        {
            var attr = attached ? " checked " : string.Empty;

            return MvcHtmlString.Create(attr);
        }
    }
}