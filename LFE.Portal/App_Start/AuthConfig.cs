using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using LFE.Application.Services.ExternalProviders;
using LFE.Core.Utils;
using LFE.Dto.Mapper.DtoMappers;
using Microsoft.Web.WebPages.OAuth;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public static class AuthConfig
    {

        public static Dictionary<string, object> FacebooksocialData = new Dictionary<string, object>();

        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            // FacebooksocialData.Add("Icon", "~/Areas/UserPortal/Content/images/btn-bf-login.png");
            // FacebooksocialData.Add("scope", "email,user_birthday,publish_stream"); //new[]{ "email","user_birthday","publish_stream"}


            OAuthWebSecurity.RegisterClient(
                    new FacebookScopedClient(
                        Constants.FB_APP_ID,
                        Constants.FB_APP_SECRET,
                        Constants.FB_SCOPE),
                    "Facebook",
                    null);

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: FB_APP_ID,
            //    appSecret: FB_APP_SECRET,
            //    displayName: "Facebook",
            //    extraData: FacebooksocialData);

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }

    public class FacebookScopedClient : IAuthenticationClient
    {
        private readonly FacebookServices _facebookServices;

        public string ProviderName
        {
            get { return "facebook"; }
        }

        private readonly string appId;
        private string appSecret;
        private readonly string scope;

        private const string baseUrl = "https://www.facebook.com/dialog/oauth?client_id=";
        //public const string graphApiToken = "https://graph.facebook.com/oauth/access_token?";
        //public const string graphApiMe = "https://graph.facebook.com/me?";

        

        public FacebookScopedClient()
        {
            appId             = Constants.FB_APP_ID;
            appSecret         = Constants.FB_APP_SECRET;
            scope             = Constants.FB_SCOPE;
            _facebookServices = new FacebookServices();// DependencyResolver.Current.GetService<FacebookServices>();
        }

        public FacebookScopedClient(string appId, string appSecret, string scope)
        {
            this.appId        = appId;
            this.appSecret    = appSecret;
            this.scope        = scope;
            _facebookServices = DependencyResolver.Current.GetService<FacebookServices>();
        }

        public void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            var url = baseUrl + appId + "&redirect_uri=" + HttpUtility.UrlEncode(returnUrl.ToString()) + "&scope=" + scope;
            context.Response.Redirect(url);
        }

        public AuthenticationResult VerifyAuthentication(HttpContextBase context)
        {
            var code = context.Request.QueryString["code"];

            if (context.Request.Url == null) return null;

            var rawUrl = context.Request.Url.AbsoluteUri;
            //From this we need to remove code portion
            rawUrl = Regex.Replace(rawUrl, "&code=[^&]*", "");


            string error;
            var accessToken = _facebookServices.GetAccessTokenFromCode(code, rawUrl, out error);

            if (String.IsNullOrEmpty(accessToken))
                return new AuthenticationResult(false, ProviderName, null, null, null);

            var userToken = _facebookServices.GetFbUserToken(accessToken, out error);

            if (userToken == null)
                return new AuthenticationResult(false, ProviderName, null, null, null);

            var result = new AuthenticationResult(true, ProviderName, userToken.id.ToString(), userToken.email, userToken.FbResponceToken2Dictionary(accessToken));//

            return result;
        }

        public AuthenticationResult VerifyAuthentication(string accessToken)
        {
            string error;

            var userToken = _facebookServices.GetFbUserToken(accessToken, out error);

            if (userToken == null)
                return new AuthenticationResult(false, ProviderName, null, null, null);

            var result = new AuthenticationResult(true, ProviderName, userToken.id.ToString(), userToken.email, userToken.FbResponceToken2Dictionary(accessToken));//

            return result;
        }
    }
}