using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using LFE.Application.Services.Interfaces;
using LFE.Cach.Provider;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Infrastructure.NLogger;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using WebMatrix.WebData;
using LFE.Portal.Areas.Widget.Models;
using UploadS3.Models;

namespace LFE.Portal.Helpers
{
    public static class Extensions
    {
        private static readonly NLogLogger _logger                        = new NLogLogger();
        private static readonly ICacheService _cacheProxy                 = DependencyResolver.Current.GetService<ICacheService>();
        private static readonly IUserAccountServices _userAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
        private static readonly IGeoServices _geoServices                 = DependencyResolver.Current.GetService<IGeoServices>();
        private static readonly IS3Wrapper _s3Wrapper                     = DependencyResolver.Current.GetService<IS3Wrapper>();
        private static List<CountryDTO> _activeCountries                  = new List<CountryDTO>();
        

        public static  BaseCurrencyDTO BASE_USD_DTO = new BaseCurrencyDTO
                                                        {
                                                            CurrencyId = Constants.DEFAULT_CURRENCY_ID
                                                            ,ISO = "USD"
                                                            ,Symbol = "$"
                                                        };

        #region user

        #region user claims
        private static Claim GetClaim(string type)
        {
            try
            {
                return ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == type);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static UserDTO claim2UserDto()
        {
            try
            {

                var claimId = GetClaim(ClaimTypes.NameIdentifier);

                if (claimId == null) return null;

                var userId = Convert.ToInt32(claimId.Value);

                var claimFirst = GetClaim(ClaimTypesHelper.CLAIM_FIRSTNAME);

                var firstName = claimFirst != null ? claimFirst.Value : string.Empty;

                var claimLast = GetClaim(ClaimTypes.Surname);

                var lastName = claimLast != null ? claimLast.Value : string.Empty;

                var claimFull = GetClaim(ClaimTypesHelper.CLAIM_FULLNAME);

                var fullName = claimFull != null ? claimFull.Value : string.Empty;

                var claimNick = GetClaim(ClaimTypesHelper.CLAIM_FIRSTNAME);

                var nick = claimNick != null ? claimNick.Value : string.Empty;

                var claimPayout = GetClaim(ClaimTypesHelper.CLAIM_PAYOUT_SETTINGS);

                var isPayoutDefinded = false;

                if (claimPayout != null) Boolean.TryParse(claimPayout.Value, out isPayoutDefinded);

                return new UserDTO
                                    {
                 
                                         UserId                 = userId//Convert.ToInt32(GetClaim(ClaimTypes.NameIdentifier).Value)
                                        ,Email                  = WebSecurity.CurrentUserName                
                                        ,FirstName              = firstName// GetClaim(ClaimTypesHelper.CLAIM_FIRSTNAME).Value
                                        ,LastName               = lastName//GetClaim(ClaimTypes.Surname).Value
                                        ,FullName               = fullName//GetClaim(ClaimTypesHelper.CLAIM_FULLNAME).Value
                                        ,Nickname               = nick// GetClaim(ClaimTypesHelper.CLAIM_NICKNAME).Value
                                        ,IsPayoutOptionsDefined = isPayoutDefinded
                                        //,UserProfileId = WebSecurity.CurrentUserId
                                    };
            }
            catch (Exception ex)
            {
                _logger.Error("claim2UserDto::" + WebSecurity.CurrentUserName, ex, CommonEnums.LoggerObjectTypes.UserAccount);
                return null;
            }              
        } 
        #endregion
        
        public static UserDTO CurrentUser(this object obj)
        {
            var email = string.Empty;
            try
            {
                if (!WebSecurity.IsAuthenticated) return null;

                var token = claim2UserDto();

                if (token != null)
                {
                    email = token.Email;
                    return token;
                }
                
                string error;

                token =  _userAccountServices.GetUserDataByEmail(WebSecurity.CurrentUserName, out error);
                
                if (token == null) return null;

                email = token.Email;

                //2013-12-26
                //_cacheProxy.Add(WebSecurity.CurrentUserId + "::userdata", token, DateTimeOffset.Now.AddDays(1));

                //create claim
                //_logger.Debug("Get Current User::token::" + _jsSerializer.Serialize(token));
                
                var securityToken = token.GetSecurityToken();

                //SessionSecurityToken sessionSecurityToken;
                //FederatedAuthentication.SessionAuthenticationModule.TryReadSessionTokenFromCookie(out sessionSecurityToken);
                //if(sessionSecurityToken!=null) 
                FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
                //_logger.Debug("Get Current User::rewrite authentication cookie::");
                FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(securityToken);   

                return token;                
            }
            catch (Exception ex)
            {
                _logger.Error("Get Current User::" + email,ex,CommonEnums.LoggerObjectTypes.UserAccount);
                return null;
            }            
        }

        public static void UpdateUserClaims(this object obj)
        {
            string error;
            var token = _userAccountServices.GetUserDataByEmail(WebSecurity.CurrentUserName, out error);

            if (token == null) return;
            var securityToken = token.GetSecurityToken();
            FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(securityToken);
        }

        public static SessionSecurityToken GetSecurityToken(this UserDTO userToken)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userToken.UserId.ToString(CultureInfo.InvariantCulture)),                     
                    new Claim(ClaimTypes.Name, userToken.Email),
                    new Claim(ClaimTypes.Email, userToken.Email),
                    new Claim(ClaimTypesHelper.CLAIM_PAYOUT_SETTINGS, userToken.IsPayoutOptionsDefined.ToString())
                };

                if (!String.IsNullOrEmpty(userToken.LastName))
                {
                    claims.Add(new Claim(ClaimTypes.Surname, userToken.LastName));                
                }

                if (!String.IsNullOrEmpty(userToken.FirstName))
                {
                    claims.Add(new Claim(ClaimTypesHelper.CLAIM_FIRSTNAME, userToken.FirstName));                
                }
 
                if (!String.IsNullOrEmpty(userToken.FullName))
                {
                    claims.Add(new Claim(ClaimTypesHelper.CLAIM_FULLNAME, userToken.FullName));                
                }

                if (!String.IsNullOrEmpty(userToken.Nickname))
                {
                    claims.Add(new Claim(ClaimTypesHelper.CLAIM_NICKNAME, userToken.Nickname));                
                }
                    

            var identity = new ClaimsIdentity(claims, "Forms");
            var principal = new ClaimsPrincipal(identity);

            return new SessionSecurityToken(principal, TimeSpan.FromDays(2));
        }

        public static UserDTO NotNullableCurrentUser(this object obj)
        {
            var u = CurrentUser(null);
            return u ?? new UserDTO();
        }

        public static string CurrentUserFullName(this object obj)
        {
            return NotNullableCurrentUser(null).FullName;
        }

        public static void RemoveCachedUserData(this object obj)
        {
            try
            {
                _cacheProxy.Remove(WebSecurity.CurrentUserId + "::userdata");
            }
            catch (Exception ex)
            {
                 _logger.Error("Remove cached user token",ex,CommonEnums.LoggerObjectTypes.UserAccount);
            }
        }

        public static void CachUserData(this object obj,UserDTO token)
        {
            try
            {
                _cacheProxy.Add(token.UserProfileId + "::userdata", token, DateTimeOffset.Now.AddDays(1));
            }
            catch (Exception ex)
            {
                _logger.Error("Cache user token",ex,CommonEnums.LoggerObjectTypes.UserAccount);
            }          
        }
        
        public static bool IsCurrentUserAdmin(this object obj)
        {
            if (!WebSecurity.IsAuthenticated) return false;

            return Roles.IsUserInRole(WebSecurity.CurrentUserName, CommonEnums.UserRoles.Admin.ToString()) || Roles.IsUserInRole(WebSecurity.CurrentUserName, CommonEnums.UserRoles.System.ToString()); 
        }

        public static bool IsPayoutTypeDefined(this object obj)
        {
            var user = obj.CurrentUser();
            return user != null && user.IsPayoutOptionsDefined;
        }
        #endregion

        public static string BaseUrl(this object obj)
        {
            return Utils.GetKeyValue("baseUrl");
        }
        public static string UploaderOrigin(this object obj)
        {
            return Utils.GetKeyValue("uploaderIframeUrl");
        }
        public static bool IsUnderSsl(this object obj)
        {
            try
            {
                var request = HttpContext.Current.Request;

                var isSsl = request.IsSecureConnection || request.Url.Scheme.ToLower() == "https";

                return isSsl;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string DecimalFormat(this decimal? value)
        {
            return (value ?? 0).ToString("0.00", CultureInfo.InvariantCulture);
        }

        public static string DecodeUrl(this string url)
        {
            return String.IsNullOrEmpty(url) ? string.Empty : HttpUtility.HtmlDecode(url);
        }

        #region course viewer links
        public static string GenerateCoursePageUrl(this object obj, string authorName, string courseName,string trackingId, string mode = null)
        {
            return authorName.GenerateItemPageUrl(authorName, courseName, BillingEnums.ePurchaseItemTypes.COURSE, trackingId, mode);
        }

        public static string GenerateBundlePageUrl(this object obj, string authorName, string bundleName, string trackingId,string mode = null)
        {
            return authorName.GenerateItemPageUrl(authorName, bundleName, BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId, mode);
        }

        public static string GenerateCourseFullPageUrl(this object obj, string authorName, string courseName,string trackingId, string mode = null)
        {

            var relative = authorName.GenerateItemPageUrl(authorName, courseName, BillingEnums.ePurchaseItemTypes.COURSE, trackingId,mode);

            if (String.IsNullOrEmpty(relative)) return string.Empty;

            return Utils.GetKeyValue("baseUrl") + relative.Remove(0, 1);
        }

        public static string GenerateBundleFullPageUrl(this object obj, string authorName, string bundleName,string trackingId, string mode = null)
        {
            var relative = authorName.GenerateItemPageUrl(authorName, bundleName, BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId, mode);

            if (String.IsNullOrEmpty(relative)) return string.Empty;

            return Utils.GetKeyValue("baseUrl") + relative.Remove(0, 1);
        }
        //public static string GenerateCoursePageUrl(this object obj, string authorName, string courseName,string mode = null, string trackingId = null)
        //{
        //    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
        //    return url.RouteUrl("UserPortal_course",new { authorName=authorName.OptimizedUrl(), courseName=courseName.OptimizedUrl(), mode, trackingId});         
        //}

        //public static string GenerateBundlePageUrl(this object obj, string authorName, string bundleName, string mode = null, string trackingId = null)
        //{
        //    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
        //    return url.RouteUrl("UserPortal_bundle", new { authorName = authorName.OptimizedUrl(), bundleName = bundleName.OptimizedUrl(), mode, trackingId });
        //}

        //public static string GenerateCourseFullPageUrl(this object obj, string authorName, string courseName, string mode = null)
        //{
        //    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

        //    var relative = url.RouteUrl("UserPortal_course", new { authorName = authorName.OptimizedUrl(), courseName = courseName.OptimizedUrl(), mode });

        //    if (String.IsNullOrEmpty(relative)) return string.Empty;

        //    return Utils.GetKeyValue("baseUrl")  + relative.Remove(0,1);
        //}

        //public static string GenerateBundleFullPageUrl(this object obj, string authorName, string bundleName, string mode = null)
        //{
        //    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            
        //    var relative =  url.RouteUrl("UserPortal_bundle", new { authorName = authorName.OptimizedUrl(), bundleName = bundleName.OptimizedUrl(), mode });

        //    if (String.IsNullOrEmpty(relative)) return string.Empty;

        //    return Utils.GetKeyValue("baseUrl") + relative.Remove(0, 1);
        //}
        #endregion

        public static string GetS3FileUrl(this object obj,string filename)
        {
            return _s3Wrapper.GetFileURL(filename);
        }

        #region billing
        public static List<SelectListItem> Months_LOV(this object obj)
        {
            var months = new List<SelectListItem>();

            for (var i = 1; i <=12; i++)
            {
                months.Add(new SelectListItem
                {
                    Value = i.ToString()
                    ,Text = i < 10 ? String.Format("0{0}",i) : i.ToString()
                });
            }

            return months;
        }
        public static List<SelectListItem> Years_LOV(this object obj)
        {
            var years = new List<SelectListItem>();

            var currnetYear = DateTime.Now.Year;

            for (var i = currnetYear; i <= currnetYear + 10; i++)
            {
                years.Add(new SelectListItem
                {
                    Value = i.ToString()
                    ,Text = i.ToString()
                });
            }

            return years;
        }
        #endregion

        #region geo

        public static DateTime ToUtcDateTime(this object obj)
        {
            return obj.UtcDateTime();
        }
        public static CountryDTO[] ActiveCountriesLOV(this object obj)
        {
            if (_activeCountries.Count > 0) return _activeCountries.ToArray();

            _activeCountries = _geoServices.ActiveCountries().OrderBy(x => (x.Index ?? 9999)).ThenBy(x=>x.CountryName).ToList();

            return _activeCountries.ToArray();
        }        

        public static bool HasStates(this object obj, int? countryId)
        {
            return countryId != null && Constants.COUNTRIES_WITH_STATES.Contains((int)countryId);
        }
        #endregion

        #region currencies
        public static List<BaseCurrencyDTO> ActiveCurrencies(this object obj)
        {
          return _geoServices.ActiveCurrenciesList; 
        }
        #endregion

        #region widget helpers
        public static string GetWixCompId(this object obj)
        {
            try
            {
                var qs = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.AbsoluteUri);

                var qrs = HttpContext.Current.Request.QueryString["compId"];

                var compId =  String.IsNullOrEmpty(qrs) ? (qs.Count.Equals(0) ? string.Empty : qs["compId"] ) : qrs;

                return compId;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static int? ItemProductToken2ItemId(this ItemProductPageToken token, BillingEnums.ePurchaseItemTypes type)
        {
            return token.ItemType == type ? token.ItemId : (int?)null;
        }
        public static int? ItemViewerToken2ItemId(this ItemViewerPageToken token, BillingEnums.ePurchaseItemTypes type)
        {
            return token.ItemType == type ? token.ItemId : (int?)null;
        }        
        #endregion

        #region others
        public static string JwPlayerKey(this object obj)
        {
            return Utils.GetKeyValue("JWPlayerSelfHostedKey");
        }

        public static string Policy(S3Config config)
        {
            var policyJson = new JavaScriptSerializer().Serialize(new
            {
                expiration = DateTime.UtcNow.Add(config.ExpirationTime).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                conditions = new object[] {
                    new { bucket = config.Bucket },
                    new [] { "starts-with", "$key", config.KeyPrefix },
                    new { acl = config.Acl },
                    new [] { "starts-with", "$success_action_redirect", "" },
                    new [] { "starts-with", "$Content-Type", config.ContentTypePrefix },
                    new Dictionary<string, string> {{ "x-amz-meta-uuid", config.Uuid.ToString() }}
                }
            });

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(policyJson));
        }

        public static string Sign(string text, string key)
        {
            var signer = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            return Convert.ToBase64String(signer.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        public static List<SelectListItem> ToNumberList(this int cnt)
        {

            var list = new List<SelectListItem>();

            for (var i = 1; i <= cnt; i++)
            {
                list.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            return list;
        }
        public static NameValue[] ObjectStateStatuses(this object obj)
        {
            return (CommonEnums.eAdminStatuses.ACTIVE | CommonEnums.eAdminStatuses.INACTIVE | CommonEnums.eAdminStatuses.PENDING | CommonEnums.eAdminStatuses.SUSPEND).ToTranslatedArray();
        }
        #endregion
    }
}