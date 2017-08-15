using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Infrastructure.NLogger;

namespace LFE.Application.Services.Helper
{
    public static class WixExtensions
    {
        private static readonly string appSecretkey = Utils.GetKeyValue("WixSecretKey");
        public static JavaScriptSerializer JSSerializer { get; private set; }
        private static readonly NLogLogger _logger = new NLogLogger();
        static WixExtensions()
        {
            JSSerializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
        }

        public static WixInstanceDTO DecodeInstance2WixInstanceDTO(this string instance,out string error)
        {
            try
            {
                var instanceParts = instance.Split('.');
                var sig = ConvertBase64ToString(instanceParts[0].Replace("_", "/").Replace("-", "+") + "=",out error);

                if (String.IsNullOrEmpty(sig)) return null;

                var encodedJson = instanceParts[1];

                //validate signature
                var hash                = new HMACSHA256(Encoding.ASCII.GetBytes(appSecretkey));
                var computeHash         = hash.ComputeHash(Encoding.ASCII.GetBytes(encodedJson));
                var s                   = Encoding.ASCII.GetString(computeHash);
                var isSignatureVerified = sig == s;

                //decode instance
                if (isSignatureVerified)
                {
                    var decodedJson = ConvertBase64ToString(instanceParts[1],out error);

                    if (String.IsNullOrEmpty(decodedJson)) return null;

                    var token = JSSerializer.Deserialize<WixInstanceDTO>(decodedJson);
                    
                    token.instanceToken = instance;

                    return token;
                    //return String.IsNullOrEmpty(decodedJson) ? null : JSSerializer.Deserialize<WixInstanceDTO>(decodedJson);
                }
                
                error = "Signature not valid";

                return null;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                _logger.Error("DecodeInstance2WixInstanceDTO::" + instance,ex,CommonEnums.LoggerObjectTypes.Wix);
                return null;
            }
        }

        public static string ConvertBase64ToString(this string toDecode, out string error)
        {
            error = string.Empty;
            try
            {
                var formated = toDecode.FormatBase64String();

                var encodedDataAsBytes = Convert.FromBase64String(formated);
                var returnValue = Encoding.ASCII.GetString(encodedDataAsBytes);
                return returnValue;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                _logger.Error("ConvertBase64ToString::" + toDecode, ex, CommonEnums.LoggerObjectTypes.Wix);
                return string.Empty;
            }
        }

        public static string FormatBase64String(this string str)
        {
            try
            {
                if (str.Length % 4 > 0) str = str.PadRight(str.Length + 4 - str.Length % 4, '=');

                return str;
            }
            catch (Exception ex)
            {
                _logger.Error("FormatBase64String::" + str, ex, CommonEnums.LoggerObjectTypes.Wix);
                return string.Empty;
            }
        }
    }
}
