using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Kendo.Mvc.UI.Fluent;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.Portal.Models;

namespace LFE.Portal.Helpers
{
    public static class HtmlExtensions
    {
        #region registration success helper
        private const string SUCCES_REGISTRATION_MSG_HEADER = "Thanks for registering {0} !";
        private const string SUCCES_REGISTRATION_MSG_BODY = "A confirmation Email was sent to the Email address you provided.<br/> Once received, click on the account activation link to complete your registration";

        public static string SuccessRegistrationMsgHeader(this HtmlHelper html, string userName)
        {
            return string.Format(SUCCES_REGISTRATION_MSG_HEADER, userName);
        }

        public static string SuccessRegistrationMsgBody(this HtmlHelper html)
        {
            return SUCCES_REGISTRATION_MSG_BODY;
        }
        #endregion

        public static string ActionString(this UrlHelper helper, string action, string controller)
        {
            helper.RemoveRoutes(helper.RequestContext.RouteData.Values);

            var url = helper.Action(action, controller);
            return url;
        }

        public static string ActionString(this UrlHelper helper, string action, string controller, string areaName)
        {
            helper.RemoveRoutes(helper.RequestContext.RouteData.Values);

            var url = helper.Action(action, controller, new { area = areaName });
            return url;
        }

        public static string ActionString(this UrlHelper helper, string action, string controller, RouteValueDictionary routData)
        {
            helper.RemoveRoutes(helper.RequestContext.RouteData.Values);

            var url = helper.Action(action, controller, routData);
            return url;
        }



        public static string ToCurrentUrl(this UrlHelper basic)
        {
            var result = HttpContext.Current.Request.UrlReferrer != null && !string.IsNullOrEmpty(HttpContext.Current.Request.UrlReferrer.AbsoluteUri) ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri;
            return result;
        }

        public static string CoursePageUrl(this UrlHelper basic, string authorName, string courseName, string mode = null)
        {
            //return Utils.BuildHttpAddress() + "courses/" + authorName + "/" + courseName + "?preview=1";
            //var u = new UrlHelper(HttpContext.Current.Request.RequestContext);
            //return u.Action("CourseViewer", "Course", new { Area = "UserPortal", id = courseName.OptimizedUrl(), mode = "preview" });
            return Extensions.GenerateCoursePageUrl(null, authorName, courseName, null, mode);
        }

        public static string ToApiUrl(this UrlHelper basic, string controller = null, string action = null)
        {

            var baseUrl = Utils.GetKeyValue("apiUrl");

            if (string.IsNullOrEmpty(controller)) return baseUrl;

            baseUrl += $"{controller.TrimString().ToLower()}/";

            if (!string.IsNullOrEmpty(action)) baseUrl += $"{action.TrimString()}/";

            return baseUrl;
        }

        public static string BundlePageUrl(this UrlHelper basic, string authorName, string bundleName, string mode = null)
        {
            //return Utils.BuildHttpAddress() + "courses/" + authorName + "/" + courseName + "?preview=1";
            //var u = new UrlHelper(HttpContext.Current.Request.RequestContext);
            //return u.Action("CourseViewer", "Course", new { Area = "UserPortal", id = courseName.OptimizedUrl(), mode = "preview" });
            return Extensions.GenerateBundlePageUrl(null, authorName, bundleName, null, mode);
        }


        public static string ToKendoVersionUrl(this UrlHelper basic)
        {
            return Utils.GetKeyValue("kendoVersion");
        }

        public static string ToWixSdkVersion(this UrlHelper basic)
        {
            return Utils.GetKeyValue("wixSdkVersion");
        }


        public static bool UseWebStoreApi(this HtmlHelper basic)
        {
            return Convert.ToBoolean(Utils.GetKeyValue("useWebStoreApi"));
        }

        public static string ToContentFullPath(this UrlHelper basic, string contentPath)
        {
            var requestUrl = basic.RequestContext.HttpContext.Request.Url;

            var absPath = VirtualPathUtility.ToAbsolute(contentPath);

            if (requestUrl == null) return string.Empty;

            var result = string.Format("{0}://{1}{2}", requestUrl.Scheme, requestUrl.Authority, absPath);
            return result;
        }

        public static bool IsDebugMode(this HtmlHelper basic)
        {
            return Convert.ToBoolean(Utils.GetKeyValue("isDebugMode"));
        }

        public static MvcHtmlString Required(this HtmlHelper htmlHelper, string additionalClass = null)
        {
            var span = "<span class='required " + (string.IsNullOrEmpty(additionalClass) ? "" : additionalClass) + "'>*</span>";

            return MvcHtmlString.Create(span);
        }

        public static MvcHtmlString TendencyDirection2CssClass(this HtmlHelper htmlHelper, ReportEnums.eTendencyDirections direction, string prefix = null)
        {
            var cssClass = "";

            switch (direction)
            {
                case ReportEnums.eTendencyDirections.Up:
                    cssClass = "up";
                    break;
                case ReportEnums.eTendencyDirections.Down:
                    cssClass = "down";
                    break;
                case ReportEnums.eTendencyDirections.Equal:
                    cssClass = "eq";
                    break;
            }

            return MvcHtmlString.Create(prefix == null ? cssClass : string.Format("{0}{1}", prefix, cssClass));
        }

        public static MvcHtmlString MenuLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributesForCurrentMenu, object htmlAttributesForAlternativeMenu)
        {

            var currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            var currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

            var current = false;

            if (actionName == currentAction && controllerName == currentController)
            {
                current = true;
            }
            else if (actionName == "Dashboard" && controllerName == "Author" && currentAction == "Index" && currentController == "Home") // default dashboard
            {
                current = true;
            }
            else if (actionName == "Courses" && controllerName == "Author" && currentAction == "EditCourse" && currentController == "Course") // edit course
            {
                current = true;
            }

            return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, current ? htmlAttributesForCurrentMenu : htmlAttributesForAlternativeMenu);
        }

        public static DropDownListBuilder EnumComboBoxFor<TModel, TEnum>(this WidgetFactory<TModel> widgetFactory, Expression<Func<TModel, TEnum>> selectedValue, TEnum value, string eventSelect, string eventDataBound, object htmlAttribuets)
        {
            var list = new List<ComboListItem>();

            var items = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            var i = 0;
            var selectedIndex = -1;

            foreach (var @enum in items)
            {
                var name = Enum.GetName(typeof(TEnum), @enum);

                if (value.ToString() == name) selectedIndex = i;

                if (name != null && (name.ToLower() == "unknown" || name.ToLower() == "undefined" || name.ToLower() == "deleted")) continue;

                var val = (int)Enum.Parse(typeof(TEnum), @enum.ToString(), true);

                list.Add(new ComboListItem
                {
                    Value = val.ToString(),
                    Text = name,
                    Index = i
                });

                i++;
            }

            //      var selectedItem = list.SingleOrDefault(x => x.Value.Equals(value)); 
            //   selectedIndex = ( selectedItem == null ) ? -1 : ( selectedItem.Index - 1 ); //first is unknown

            var widget = widgetFactory.DropDownListFor(selectedValue)
                .BindTo(list)
                .SelectedIndex(selectedIndex)
                .DataTextField("Text")
                .DataValueField("Value");

            if (!string.IsNullOrEmpty(eventDataBound))
            {
                // ReSharper disable once MustUseReturnValue
                widget.Events(e => e.DataBound(eventDataBound));
            }

            if (!string.IsNullOrEmpty(eventSelect))
            {
                // ReSharper disable once MustUseReturnValue
                widget.Events(e => e.Select(eventSelect));
            }


            if (htmlAttribuets != null)
            {
                // ReSharper disable once MustUseReturnValue
                widget.HtmlAttributes(htmlAttribuets);
            }

            return widget;
        }

        public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var names = Enum.GetNames(metaData.ModelType);
            var sb = new StringBuilder();
            foreach (var name in names)
            {
                if (name.ToLower() == "unknown" || name.ToLower() == "undefined") continue;
                var description = name;

                var memInfo = metaData.ModelType.GetMember(name);
                {
                    var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes.Length > 0)
                        description = ((DescriptionAttribute)attributes[0]).Description;
                }

                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metaData.PropertyName,
                    name
                    );

                var radio = htmlHelper.RadioButtonFor(expression, name, new { id }).ToHtmlString();
                sb.AppendFormat(
                    "{2} <label for=\"{0}\">{1}</label>",
                    id,
                    HttpUtility.HtmlEncode(description),
                    radio
                    );
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        public static string ToHtml(this Controller basic, string viewToRender, object model /*ViewDataDictionary viewData*/ )
        {
            var result = ViewEngines.Engines.FindView(basic.ControllerContext, viewToRender, null);
            basic.ViewData.Model = model;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(basic.ControllerContext, result.View, basic.ViewData /*viewData*/, basic.TempData, output);
                result.View.Render(viewContext, output);
                result.ViewEngine.ReleaseView(basic.ControllerContext, result.View);

                return output.ToString();
            }

        }
    }
}