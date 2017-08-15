using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using LFE.Model;

namespace LFE.Application.Services.Helper
{
    public static class ServiceExtensions
    {
        #region email services
        public static string GetParentURL(this string activationLinkHref, string parentWindowURL = null)
        {
            if (String.IsNullOrEmpty(parentWindowURL)) return activationLinkHref;

            var hash = "";
            if (parentWindowURL.Contains("#"))
            {
                hash = parentWindowURL.Substring(parentWindowURL.IndexOf("#", StringComparison.Ordinal));
            }

            var lfeUrlIndex = parentWindowURL.ToLower().IndexOf("&lfe_app_url=", StringComparison.Ordinal);
            if (lfeUrlIndex > 0)
            {
                parentWindowURL = parentWindowURL.Substring(0, lfeUrlIndex);
            }
            activationLinkHref = parentWindowURL + ( parentWindowURL.Contains('?') ? "" : "?" ) + "&lfe_app_url=" + HttpContext.Current.Server.UrlEncode(activationLinkHref) + hash;

            return activationLinkHref;
        }        
        #endregion

        #region user account extensions
        public static string ErrorCodeToString(this MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        } 
        #endregion

        #region Brightcove 
   
        public static UserVideoDto VideoEntity2VideoDTO(this USER_Videos video, int userId,int uses)
        {
            if(video==null) return new UserVideoDto{userId = userId};

            
            var token = new UserVideoDto
            {
                userId     = userId
               ,identifier = video.BcIdentifier.ToString()
               ,bcid       = video.BcIdentifier.ToString()
               ,name       = video.Name
               ,title      = video.ShortDescription
               ,views      = video.PlaysTotal
               ,uses       = uses
               ,duration   = video.Length.VideoLength2Duration()
               ,minutes    = video.Length.VideoLength2Duration().Duration2HoursString()
               ,thumbUrl   = video.BcIdentifier.CombimeVideoUrl(userId,CommonEnums.eVideoPictureTypes.Thumb).ToCloudfrontSignedUrl()
               ,stillUrl   = video.BcIdentifier.CombimeVideoUrl(userId,CommonEnums.eVideoPictureTypes.Still).ToCloudfrontSignedUrl()
               ,addon      = video.CreationDate
               ,tags       = string.IsNullOrEmpty(video.Tags) ? new string[0] : video.Tags.Split(Convert.ToChar(","))
               ,millisec   = video.Length
               ,status     = ImportJobsEnums.eFileInterfaceStatus.Transferred               
            };

            token.tagsStr = token.tags.StringsList2String();

            return token;
        }

        #endregion

        #region course
        //public static decimal? CourseEntity2Price(this Courses entity, bool isSubscription)
        //{
        //    return isSubscription ? (entity.MonthlySubscriptionPriceUSD ?? 0) : entity.PriceUSD;
        //}

        //public static decimal? BundleEntity2Price(this CRS_Bundles entity, bool isSubscription)
        //{
        //    return isSubscription ? (entity.MonthlySubscriptionPrice ?? 0) : entity.Price;
        //}
        #endregion

        #region common

        public static DateTime NextMonthFirst(this object obj)
        {
            var now = DateTime.Now;

            return new DateTime(now.AddMonths(1).Year, now.AddMonths(1).Month, 1);
        }
      
        public static bool IsObjectNameValid(this string name, out string error )
        {
            error = String.Empty;

            if (String.IsNullOrEmpty(name))
            {
                error = "Name required";
                return false;
            }

            try
            {
                var rgx = new Regex(Constants.REGEX_NAME_FORMAT);

                if(rgx.IsMatch(name)) return true;

                error = Constants.REGEX_NAME_FORMAT_ERROR;

                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }
        #endregion
    }
}
