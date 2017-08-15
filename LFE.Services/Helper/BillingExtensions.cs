using System.Globalization;
using System.Web;
using System.Web.Script.Serialization;
using LFE.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LFE.Core.Utils;
using LFE.DataTokens;
using PayPal.Api;
using PayPal.Exception;
using PayPal.PayPalAPIInterfaceService.Model;

namespace LFE.Application.Services.Helper
{
    public static class BillingExtensions
    {
        private static readonly bool _isDaily = Boolean.Parse(Utils.GetKeyValue("useTestDailySubscription"));  
        private static readonly JavaScriptSerializer _jsSerializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

        private const string cardRegex = "^(?:(?<Visa>4\\d{3})| (?<MasterCard>5[1-5]\\d{2})|(?<Discover>6011)|(?<DinersClub> (?:3[68]\\d{2})|(?:30[0-5]\\d))|(?<Amex>3[47]\\d{2}))([ -]?)(?(DinersClub)(?:\\d{6}\\1\\d{4})|(?(Amex)(?:\\d{6}\\1\\d{5})|(?:\\d{4}\\1\\d{4}\\1\\d{4})))$";

        public static bool PassesLuhnTest(string cardNumber)
        {
            //Clean the card number- remove dashes and spaces
            cardNumber = cardNumber.Replace("-", "").Replace(" ", "");

            //Convert card number into digits array
            var digits = new int[cardNumber.Length];
            for (var len = 0; len < cardNumber.Length; len++)
            {
                digits[len] = Int32.Parse(cardNumber.Substring(len, 1));
            }

            //Luhn Algorithm
            //Adapted from code available on Wikipedia at
            //http://en.wikipedia.org/wiki/Luhn_algorithm
            var sum = 0;
            var alt = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var curDigit = digits[i];
                if (alt)
                {
                    curDigit *= 2;
                    if (curDigit > 9)
                    {
                        curDigit -= 9;
                    }
                }
                sum += curDigit;
                alt = !alt;
            }

            //If Mod 10 equals 0, the number is good and this will return true
            return sum % 10 == 0;
        }

        public static BillingEnums.eCreditCardType? GetCardTypeFromNumber(string cardNum)
        {
            //Create new instance of Regex comparer with our
            //credit card regex patter
            var cardTest = new Regex(cardRegex);

            //Compare the supplied card number with the regex
            //pattern and get reference regex named groups
            var gc = cardTest.Match(cardNum).Groups;

            //Compare each card type to the named groups to 
            //determine which card type the number matches
            if (gc[BillingEnums.eCreditCardType.Amex.ToString()].Success)
            {
                return BillingEnums.eCreditCardType.Amex;
            }
            if (gc[BillingEnums.eCreditCardType.MasterCard.ToString()].Success)
            {
                return BillingEnums.eCreditCardType.MasterCard;
            }
            if (gc[BillingEnums.eCreditCardType.Visa.ToString()].Success)
            {
                return BillingEnums.eCreditCardType.Visa;
            }
            if (gc[BillingEnums.eCreditCardType.Discover.ToString()].Success)
            {
                return BillingEnums.eCreditCardType.Discover;
            }
            //Card type is not supported by our system, return null
            //(You can modify this code to support more (or less)
            // card types as it pertains to your application)
            return null;
        }

        public static bool IsValidNumber(string cardNum, BillingEnums.eCreditCardType? cardType)
        {
            //Create new instance of Regex comparer with our 
            //credit card regex pattern
            var cardTest = new Regex(cardRegex);

            //Make sure the supplied number matches the supplied
            //card type
            return cardTest.Match(cardNum).Groups[cardType.ToString()].Success && PassesLuhnTest(cardNum);
        }

        public static bool IsValidNumber(string cardNum)
        {
            //Determine the card type based on the number
            var cardType = GetCardTypeFromNumber(cardNum);

            //Call the base version of IsValidNumber and pass the 
            //number and card type
            return IsValidNumber(cardNum, cardType);
        }

        //
        public static string ItemName2SubscriptionDescription(this string itemName)
        {
            return String.Format("{0} Monthly subscription on {1}", _isDaily ? "(daily) " : "", itemName).TrimString();
        }

        public static string ItemName2CoursePurchaseDescription(this string itemName)
        {
            return String.Format("Purchase {0}", itemName);
        }

        public static decimal CalculateInitialCourseMonthlySubscriptionAmount(this decimal amount)
        {
            try
            {
                var now = DateTime.Now;
                var today = (decimal)now.Day;
                var days = (decimal)DateTime.DaysInMonth(now.Year, now.Month);

                return Math.Round((amount / days) * Math.Max((days - today),1), 2);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static DateTime ToBillingStartDate(this DateTime date)
        {
            var next = date.AddMonths(1);
            return new DateTime(next.Year,next.Month,1);
        }
        //
        public static string FormatPaypalException(this PayPalException ex)
        {
            var exception = ex.InnerException as ConnectionException;
            
            if (exception == null) return Utils.FormatError(ex);
            
            try
            {
                var repsonse = exception.Response;

                var result = _jsSerializer.Deserialize<PaypalExceptionToken>(repsonse);

                if (result.details==null || result.details.Count.Equals(0)) return result.message;

                return result.details.Aggregate(String.Empty, (current, detail) => current + ((String.IsNullOrEmpty(current) ? "" : "::") + (detail.field + "-" + detail.issue)));

                //foreach (var detail in result.details)
                //{
                //    message += (string.IsNullOrEmpty(message) ? "" : "::") + (detail.field + "-" + detail.issue);
                //}
            }
            catch (Exception)
            {
                return Utils.FormatError(ex);
            }
        }

        public static string FormatMerchantApiErrors(this List<ErrorType> errors)
        {
            return errors.Aggregate(String.Empty, (current, detail) => current + ((String.IsNullOrEmpty(current) ? "" : "::") + (detail.LongMessage ?? detail.ShortMessage)));
        }

        public static string Payment2TransactionId(this Payment payment)
        {
            try
            {
                return payment.transactions[0].related_resources[0].sale.id;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        //
        public static BillingEnums.eTransactionTypes PaypalRequestType2TransactionType(this BillingEnums.ePaypalRequestTypes type)
        {
            switch (type)
            {
                case BillingEnums.ePaypalRequestTypes.ACCOUNT_PAYMENT: 
                    return  BillingEnums.eTransactionTypes.DirectPaymentTransaction;
                case BillingEnums.ePaypalRequestTypes.CC_DIRECT: 
                    return BillingEnums.eTransactionTypes.DirectCreditCardPayment;
                case BillingEnums.ePaypalRequestTypes.CC_PAY_WITH_SAVED:
                    return BillingEnums.eTransactionTypes.SavedCreditCardPayment;
                default:
                    return BillingEnums.eTransactionTypes.Undefined;
            }
        }

        public static DateTime? ToRentalEndDate(this BillingEnums.eBillingPeriodType periodType,short numberOfPeriods)
        {
            var localZone = TimeZone.CurrentTimeZone;
            var localTime = DateTime.Now;
            var utc = localZone.ToUniversalTime(localTime);

            switch (periodType)
            {
               case BillingEnums.eBillingPeriodType.HOUR:
                    return utc.AddHours(numberOfPeriods);
               case BillingEnums.eBillingPeriodType.DAY:
                    return utc.AddDays(numberOfPeriods);
               case BillingEnums.eBillingPeriodType.WEEK:
                    return utc.AddDays(numberOfPeriods*7);
               case BillingEnums.eBillingPeriodType.MONTH:
                    return utc.AddMonths(numberOfPeriods);
                default:
                    return null;
            }
        }
    }

    public static class PaypalExtensions
    {

        public static DateTime ParsePaypalDate(this string paypalDatetime, DateTime defaultDate)
        {
            DateTime date;
            return paypalDatetime.TryParsePaypalDate(out date) ? date : defaultDate;
        }

        public static bool TryParsePaypalDate(this string paypalDatetime, out DateTime date)
        {
            var formatedDate = HttpUtility.UrlDecode(paypalDatetime);

            var formats = new[] { "HH:mm:ss dd MMM yyyy PDT", "HH:mm:ss dd MMM yyyy PST", 
                                      "HH:mm:ss dd MMM, yyyy PST", "HH:mm:ss dd MMM, yyyy PDT", 
                                      "HH:mm:ss MMM dd, yyyy PST", "HH:mm:ss MMM dd, yyyy PDT" };

            return DateTime.TryParseExact(formatedDate,formats, CultureInfo.InvariantCulture,DateTimeStyles.None, out date);
        }

        public static CurrencyCodeType Currency2PaypalCurrencyCode(this BaseCurrencyDTO currency)
        {
            try
            {
                return (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currency.ISO);
            }
            catch (Exception)
            {
                return CurrencyCodeType.USD;
            }
        }
    }



    public static class LuhnCreditCardTest
    {
        public static bool LuhnCheck(this string cardNumber)
        {
            return LuhnCheck(cardNumber.Select(c => c - '0').ToArray());
        }

        static readonly int[] results = { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };

        private static bool LuhnCheck(this int[] digits)
        {
            var checkValue = GetCheckValue(digits);
            return checkValue == 0;
        }

        private static int GetCheckValue(ICollection<int> digits)
        {
            var i = 0;
            var lengthMod = digits.Count % 2;
            return digits.Sum(d => i++ % 2 == lengthMod ? results[d] : d) % 10;
        }
    }
}
