using System.Collections.Generic;
using LFE.Core.Utils;
using PayPal.PayPalAPIInterfaceService;
using APIContext           = PayPal.Api.APIContext;
using OAuthTokenCredential = PayPal.Api.OAuthTokenCredential;
using SDKVersion           = PayPal.Api.SDKVersion;

namespace LFE.Application.Services.Helper
{
    public static class PayPalConfiguration
    {
        //sandbox IPN settings
        //https://www.sandbox.paypal.com/us/cgi-bin/webscr?dispatch=***************************************

        public const string ADAPTIVE_PAYMENT_SENDER_EMAIL = "**************************";

        private static readonly string PAYPAL_MODE = Utils.GetKeyValue("PaypalMode");
        //https://developer.paypal.com/developer/applications/edit/******************************************************

        public static readonly string PAYPAL_AGREEMENT_CHECKOUT_URL = PAYPAL_MODE == "sandbox" ? "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token={0}" : "https://www.paypal.com/us/cgi-bin/webscr?cmd=_express-checkout&token={0}";
        private static readonly string API_CLIENT_ID = PAYPAL_MODE == "sandbox" ? "********************************************" : "******************************************";
        private static readonly string API_SECRET = PAYPAL_MODE == "sandbox" ? "*********************************************" : "***************************************";

        private static string ACCESS_TOKEN = string.Empty;

        //merchant sdk config
        //sandbox account for zglozman@lfe.com
        //https://developer.paypal.com/developer/accounts/
        public static readonly Dictionary<string, string> sdkConfig = new Dictionary<string, string>
        {
            {"mode",PAYPAL_MODE},
            {"account1.apiUsername",PAYPAL_MODE == "sandbox" ? "*********************" : "*************************"},
            {"account1.apiPassword",PAYPAL_MODE == "sandbox" ? "*********************" : "******************"},
            {"account1.apiSignature",PAYPAL_MODE == "sandbox" ? "*******************************" : "**********************************"},
            {"account1.applicationId",PAYPAL_MODE == "sandbox" ? "APP-**********************" : "APP-**************************"} 
        };

        //merchant api interface service
        public static PayPalAPIInterfaceServiceService PayPalMerchantAPIService => new PayPalAPIInterfaceServiceService(sdkConfig);

        // Create the configuration map that contains mode and other optional configuration details.
        private static Dictionary<string, string> GetConfig()
        {
            var configMap = new Dictionary<string, string>
            {
                {"mode", PAYPAL_MODE}
            };

            // Endpoints are varied depending on whether sandbox OR live is chosen for mode

            // These values are defaulted in SDK. If you want to override default values, uncomment it and add your value
            // configMap.Add("connectionTimeout", "360000");
            // configMap.Add("requestRetries", "1");
            return configMap;
        }

        // Create accessToken
        private static string GetAccessToken()
        {
            // ###AccessToken
            // Retrieve the access token from
            // OAuthTokenCredential by passing in
            // ClientID and ClientSecret
            // It is not mandatory to generate Access Token on a per call basis.
            // Typically the access token can be generated once and
            // reused within the expiry window                
            //if (!string.IsNullOrEmpty(ACCESS_TOKEN)) return ACCESS_TOKEN;

            ACCESS_TOKEN = new OAuthTokenCredential(API_CLIENT_ID, API_SECRET, GetConfig()).GetAccessToken();
            return ACCESS_TOKEN;
        }

        // Returns APIContext object
        public static APIContext GetAPIContext()
        {
            // ### Api Context
            // Pass in a `APIContext` object to authenticate 
            // the call and to send a unique request id 
            // (that ensures idempotency). The SDK generates
            // a request id if you do not pass one explicitly. 
            var apiContext = new APIContext(GetAccessToken())
            {
                Config = GetConfig()
                ,
                SdkVersion = new SDKVersion
                {

                }
            };

            // Use this variant if you want to pass in a request id  
            // that is meaningful in your application, ideally 
            // a order id.
            // String requestId = Long.toString(System.nanoTime();
            // APIContext apiContext = new APIContext(GetAccessToken(), requestId ));

            return apiContext;
        }

    }
}
