using System;
using System.IO;
using System.Web;
using Amazon.CloudFront;
using LFE.Application.Services.ExternalProviders;
using LFE.Core.Utils;

namespace LFE.Application.Services.Helper
{

    public static class VideoExtensions
    {
        public static string ToS3SignedUrl(this string url,string bucket)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            var _s3Wrapper = new S3Wrapper();

            return _s3Wrapper.ToS3SignedUrl(url,bucket);
        }

        private static int GetUnixTime(DateTime time)
        {
            DateTime referenceTime = new DateTime(1970, 1, 1);
            return (int)(time - referenceTime).TotalSeconds;

        }
        public static string ToCloudfrontSignedUrl(this string url)
        {
            var cloudFrontKeyPairID = Utils.GetKeyValue("CloudFrontPairID");
            var pathtokey = HttpContext.Current.Request.MapPath("~/Secure/cert/pk-cf.pem");
            var privateKey = new FileInfo(pathtokey);

            string expirationEpoch = GetUnixTime(DateTime.UtcNow.AddDays(2)).ToString();

            string policy =
             @"{""Statement"":[{""Resource"":""<url>"",""Condition"":{""DateLessThan"":{""AWS:EpochTime"":<expiration>}}}]}".
                 Replace("<url>", url).
                 Replace("<expiration>", expirationEpoch);

            var signedUrl = AmazonCloudFrontUrlSigner.SignUrl(
                url,
                cloudFrontKeyPairID,
                privateKey,
                policy);

            return signedUrl;
        }
        
        internal class CfSignedUrlResp
        {
            public string url;
        }
    }

}
