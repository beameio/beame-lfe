using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using LFE.Application.Services;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Portal.Controllers
{
    
    public class PaypalController : ApiBaseController
    {
        private static readonly string PAYPAL_MODE = Utils.GetKeyValue("PaypalMode");

        public static readonly string PAYPAL_URL = PAYPAL_MODE == "sandbox" ? "https://www.sandbox.paypal.com/cgi-bin/webscr" : "https://www.paypal.com/us/cgi-bin/webscr";
     
        private readonly IPaypalIpnServices _paypalServices;
      
        public PaypalController()
        {
            _paypalServices = DependencyResolver.Current.GetService<IPaypalIpnServices>();
        }

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.ActionName("ipn")]
        public HttpResponseMessage IPN()
        {
            try
            {
                Logger.Debug("IPN call::");

                

                var formVals = new Dictionary<string, string>
                {
                    {"cmd", "_notify-validate"}
                };

                var req = (HttpWebRequest)WebRequest.Create(PAYPAL_URL);

                // Set values for the request back
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                var param = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                var strRequest = Encoding.ASCII.GetString(param);

                var sb = new StringBuilder();
                sb.Append(strRequest);

                foreach (var key in formVals.Keys)
                {
                    sb.AppendFormat("&{0}={1}", key, formVals[key]);
                }
                strRequest += sb.ToString();
                req.ContentLength = strRequest.Length;

                //Send the request to PayPal and get the response
                using (var streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
                {
                    streamOut.Write(strRequest);
                    streamOut.Close();

                    Logger.Debug("IPN call::request::" + strRequest);

                    using (var streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                    {
                        var response = streamIn.ReadToEnd();

                        if (response == "VERIFIED")
                        {
                            string error;
                            var token = strRequest.Qs2IpnResponseToken(out error);
                            if (token == null)
                            {
                                Logger.Error("token not parsed::" + error);
                                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
                            }

                            if (token.txn_type == null || token.txn_type.ToLower() != "masspay")
                            {
                                _paypalServices.HandleIpnResponse(token);
                            }
                            else
                            {

                                var parsedQuery = HttpUtility.ParseQueryString(strRequest);

                                var payments = new Dictionary<int, Dictionary<string, string>>();
                                var queryRegex = new Regex(@"^(?<key>.+)_(?<index>\d+)");

                                // Parsing
                                foreach (var key in parsedQuery)
                                {
                                    var value = parsedQuery[key.ToString()];
                                    var match = queryRegex.Match(key.ToString());

                                    if (!match.Success) continue;

                                    var normalizedKey = match.Groups["key"];
                                    var index = int.Parse(match.Groups["index"].Value);

                                    Dictionary<string, string> collection;
                                    if (payments.ContainsKey(index))
                                    {
                                        collection = payments[index];
                                    }
                                    else
                                    {
                                        collection = new Dictionary<string, string>();
                                        payments.Add(index, collection);
                                    }

                                    if (!collection.ContainsKey(normalizedKey.ToString()))
                                    {
                                        collection.Add(normalizedKey.ToString(), value.Split(Convert.ToChar(","))[0]);
                                    }
                                }

                                var items = new List<MassPaymentItemToken>();

                                if (payments.Count > 0)
                                {
                                    foreach (var payment in payments)
                                    {
                                        var c = payment.Value;

                                        items.Add(c.Dict2PaymentItemToken());
                                    }
                                }

                                _paypalServices.HandleIpnMasspayResponse(items);
                            }
                           

                        }
                        else
                        {
                            Logger.Warn("IPN call::response not verified::" + response + "::" + strRequest );
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error("IPN listener",ex,CommonEnums.LoggerObjectTypes.PayPal);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }       
        }

       
    }

}

