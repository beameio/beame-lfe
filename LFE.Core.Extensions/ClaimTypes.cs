using System;
using System.Configuration;

namespace LFE.Core.Extensions
{
    public static class ClaimTypesHelper
    {
        public static readonly string BASE_URL               = GetKeyValue("baseUrl");
        public static readonly string CLAIM_NICKNAME         = String.Format("{0}/claims/nickname",BASE_URL);
        public static readonly string CLAIM_FULLNAME         = String.Format("{0}/claims/fullname", BASE_URL);
        public static readonly string CLAIM_FIRSTNAME        = String.Format("{0}/claims/firstname", BASE_URL);
        public static readonly string CLAIM_PAYOUT_SETTINGS  = String.Format("{0}/claims/payout", BASE_URL);

        public static string GetKeyValue(string value)
        {
            return ConfigurationManager.AppSettings.Get(value);
        }
    }
}