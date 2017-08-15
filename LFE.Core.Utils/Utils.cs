using LFE.Core.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace LFE.Core.Utils
{
	public static class Utils
	{
		#region enum services
		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}
		
		public static T ParseEnum<T>(short value)
		{
			return (T)Enum.Parse(typeof(T), value.ToString());
		}

		public static T ParseEnum<T>(byte value)
		{
			return (T)Enum.Parse(typeof(T), value.ToString());
		}

		public static string GetEnumDescription(Enum value)
		{
			string output = null;
			var type = value.GetType();

			var fi = type.GetField(value.ToString());
			var attrs = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attrs.Length > 0)
			{
				output = attrs[0].Description;
			}

			return output;
		}

		public static string Enum2Description(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : value.ToString();
		}

		public static List<NameValue> EnumWithDescToList<T>()
		{

			var array = ( Enum.GetValues(typeof(T)).Cast<T>() ).ToArray();
			var array2 = Enum.GetNames(typeof(T)).ToArray();

			var lst = new List<NameValue>();

			for (var i = 0; i < array.Length; i++)
			{				

				var name = array2[i];

				var value = array[i];

				if (value.ToString().ToLower() == "undefined" || value.ToString().ToLower() == "unknown") continue;

				var fi = value.GetType().GetField(value.ToString());
				var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				lst.Add(new NameValue { Title = attributes.Length.Equals(1) ? attributes[0].Description : name, Name = name, Value = (int)Enum.Parse(typeof(T), value.ToString(), true) });
			}
			return lst;
		}
		
		public static NameValue[] ToTranslatedArray<TEnum>(this TEnum data) where TEnum : struct, IConvertible
		{
			var names = data.ToString().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

			return (from item in names
					let fi = typeof (TEnum).GetField(item)
					let attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false)
					select new NameValue
						{
							Value = (int) Enum.Parse(typeof (TEnum), item, true),
							Name = item, 
							Title = attributes.Length > 0 ? attributes[0].Description : item
						}).ToArray();
		}

		public static CommonEnums.eRegistrationSources PluginType2RegistrationSource(this PluginEnums.ePluginType type)
		{
			switch (type)
			{
				case PluginEnums.ePluginType.WIX: return CommonEnums.eRegistrationSources.WIX;
				case PluginEnums.ePluginType.FB: return CommonEnums.eRegistrationSources.FB;
				case PluginEnums.ePluginType.WORDPRESS: return CommonEnums.eRegistrationSources.WORDPRESS;
				case PluginEnums.ePluginType.JOOMLA: return CommonEnums.eRegistrationSources.JOOMLA;
				case PluginEnums.ePluginType.DRUPAL: return CommonEnums.eRegistrationSources.DRUPAL;
				default: return CommonEnums.eRegistrationSources.LFE;
			}
		}
		#endregion

		#region urls
		public static string OptimizedUrl(this string text)
		{
			if (text == null)
			{
				return null;
			}
			
			return text
				.ToLower()
				.Replace(" ", "-")
				.Replace("%20", "-")
				.Replace("+", "-")
				.Replace("*", "-")
				.Replace("!", "-")
				.Replace("&amp;", "and") // first "&amp;" then "&" since "&" is part of "&amp;"
				.Replace("&", "and");
		}

		public static string BaseUrl()
		{
			return GetKeyValue("baseUrl");
				//String.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, "/");
		}

		public static string BuildHttpAddress()
		{
			var domainName = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
			var httpPrefix = HttpContext.Current.Request.Url.ToString().Substring(0, 5).Equals("https") ? "https://" : "http://";

			var _appName = HttpContext.Current.Request.ApplicationPath;

			var httpAddress = httpPrefix + domainName + _appName + "/";
			if (httpAddress.IndexOf("//", httpPrefix.Length, StringComparison.Ordinal) > 0)
				httpAddress = httpAddress.Remove(httpAddress.IndexOf("//", httpPrefix.Length, StringComparison.Ordinal), 1);

			return httpAddress;
		}

		public static string StringsList2String(this IEnumerable<string> list)
		{
			return string.Join(", ", list.ToArray());
		}

		public static string ToFullUrl(this string url)
		{
			return string.IsNullOrEmpty(url) ? "" : url.Replace("~/", BuildHttpAddress());
		}
		public static string ToVirtualUrl(this string url)
		{
			return string.IsNullOrEmpty(url) ? "" : url.Replace(BuildHttpAddress(), "~/").FormatUrl();
		}
		public static string ToCssClass(this string iso)
		{            
			switch (iso.ToLower())
			{
				case "usd":
					return "usd";
				case "eur":
					return "eur";
			}

			return string.Empty;
		}
		public static string FormatUrl(this string url)
		{
			return string.IsNullOrEmpty(url) ? "" : url.Replace(" ", "%20");
		}

		public static string FileName2Title(this string name)
		{
			if (string.IsNullOrEmpty(name)) return string.Empty;
			try
			{
				return Path.GetFileNameWithoutExtension(name.LastIndexOf("?", StringComparison.Ordinal) < 0 ? name : name.Remove(name.LastIndexOf("?", StringComparison.Ordinal)));
			}
			catch (Exception)
			{
				return Path.GetFileNameWithoutExtension(name);
			}
		}

		public static string Url2FileName(this string url)
		{
			if (string.IsNullOrEmpty(url)) return string.Empty;
			try
			{
				return Path.GetFileName(url.LastIndexOf("?", StringComparison.Ordinal) < 0 ? url : url.Remove(url.LastIndexOf("?", StringComparison.Ordinal)));
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string CleanFileNameFromQs(this string name)
		{
			if (string.IsNullOrEmpty(name)) return string.Empty;

			try
			{
				return Path.GetFileName(name.LastIndexOf("?", StringComparison.Ordinal) < 0 ? name : name.Remove(name.LastIndexOf("?", StringComparison.Ordinal)));
			}
			catch (Exception)
			{
				return Path.GetFileName(name);
			}
		}

		//public static bool IsUrlValid(this string url)
		//{
		//    if (String.IsNullOrEmpty(url)) return false;
		//    HttpWebResponse response =  null;
		//    try
		//    {
		//        var cl = new WebClient();

				

		//        var request = (HttpWebRequest) WebRequest.Create(url);

		//        request.Method = "HEAD";

		//        response = (HttpWebResponse) request.GetResponse();

		//        return response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0;
		//    }
		//    catch (Exception)
		//    {
		//        return false;
		//    }
		//    finally
		//    {
		//        if (response != null) response.Dispose();
		//    }
		//}
		public static string PageId2FacebookPageUrl(this long? pageId)
		{
			return pageId == null ? string.Empty : $"http://facebook.com/pages/-/{pageId}";
		}

		public static string FacebookPageUrl2AppUrl(this string pageUrl, long? pageId)
		{
			return pageId == null ? string.Empty : $"{pageUrl}?sk=app_{pageId}";
		}
		#endregion

		#region strings
		
		public static string NullableString(this string str)
		{
			return string.IsNullOrEmpty(str) ? null : str.Trim();
		}

		public static string TrimString(this string str)
		{
			return string.IsNullOrEmpty(str) ? string.Empty : str.Trim();
		}

		public static string TrimString(this string str,int maxLenght)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;
			
			var str2Return =  str.Trim();

			return str2Return.Length < maxLenght ? str2Return : str2Return.Substring(0, maxLenght - 1);
		}

		public static string FormatToFullUri(this string str)
		{
			if (string.IsNullOrEmpty(str)) return string.Empty;

			var url = str.TrimString();

			if (url.Contains("http://") || url.Contains("https://")) return url;

			return "http://" + url;

		}

		public static string EnumToLowerString<TEnum>(this TEnum val) where TEnum : struct , IConvertible
		{
			try
			{
				return val.ToString().ToLower();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string DecodeString(this string str)
		{
			return string.IsNullOrEmpty(str) ? string.Empty : HttpUtility.HtmlDecode(str.Trim());
		}

		public static string ToFullName(this string first, string last)
		{
			return $"{first} {last}";
		}

		//public static decimal FormatMoney(this decimal value)
		//{
		//    try
		//    {
		//        return Math.Round(value, 0);
		//    }
		//    catch (Exception)
		//    {
		//        return -1;
		//    }
		//}
		//public static decimal? FormatNullableMoney(this decimal? value)
		//{
		//    return value == null ? (decimal?)null : Math.Round((decimal)value, 2);
		//}

		//public static decimal FormatMoney(this decimal? value)
		//{
		//    try
		//    {
		//        return value == null ? 0 : Math.Round((decimal)value, 0);
		//    }
		//    catch (Exception)
		//    {
		//        return -1;
		//    }
		//}

		public static decimal FormatMoney(this decimal? value,int presicion)
		{
			try
			{
				return value == null ? 0 : FormatMoney((decimal)value, presicion);
			}
			catch (Exception)
			{
				return -1;
			}
		}
	  
		public static decimal FormatMoney(this decimal value, int presicion)
		{
			try
			{
				return presicion == 0 ? Math.Ceiling(value): Math.Round(value, presicion);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public static decimal FormatdDecimal(this decimal? value, int presicion)
		{
			try
			{
				return value == null ? 0 : FormatDecimal((decimal)value, presicion);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public static decimal FormatDecimal(this decimal value, int presicion)
		{
			try
			{
				return presicion == 0 ? Math.Ceiling(value) : Math.Round(value, presicion);
			}
			catch (Exception)
			{
				return -1;
			}
		}
		public static string CapitalizeWord(this string word)
		{
			if (string.IsNullOrEmpty(word)) return string.Empty;

			if (word.Length.Equals(1)) return word.ToUpper();

			return char.ToUpper(word[0]) + word.Substring(1).ToLower();
		}
		#endregion

		#region application	  
		public static short? CalculateAge(this DateTime? birth)
		{
			if (birth == null) return null;

			var birthDate = (DateTime)birth;

			var now = DateTime.Now;
			var age = DateTime.Now.Year - birthDate.Year;

			if (now.Month < birthDate.Month || ( now.Month == birthDate.Month && now.Day < birthDate.Day )) age--;
			return (short?)age;
		}
		
		#endregion

		#region common
		public static string GetKeyValue(string value)
		{
			return ConfigurationManager.AppSettings.Get(value);
		}

		public static string FormatError(Exception ex)
		{
			return ex != null ? ( ex.InnerException == null ? ex.Message : ( ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException .Message) ) : string.Empty;
		}

		public static bool Int2Boolean(this int value)
		{
			return (value == 0 || value == 1) && value == 1;
		}
		public static string FormatPrice2Str(this decimal? price, int decimals)
		{
			return price == null ? string.Empty : FormatPrice((decimal)price, decimals).ToString();
		}
		public static decimal? FormatPrice(this decimal? price,int decimals)
		{
			return price == null ?  (decimal?) null : FormatPrice((decimal)price, decimals);
		}
		public static decimal FormatPrice(this decimal price, int decimals)
		{
			return decimal.Round(price, decimals, MidpointRounding.AwayFromZero);
		}
		public static decimal FormatPrice(this decimal price)
		{
			return  FormatPrice(price,2);
		}
		public static decimal FormatPrice(this decimal? price)
		{
			return price== null ? 0 : FormatPrice((decimal)price);
		}

		public static decimal FormatByte(this byte? value)
		{
			return value == null ? 0 : FormatPrice((decimal)value);
		}

		public static decimal? FormatNullablePrice(this decimal? price)
		{
			return price == null ? (decimal?)null : price.FormatPrice();
		}

		public static decimal? ItemPrice2DisplayPrice(this decimal? price)
		{
			if (price == null) return null;

			var p = (decimal)price;

			return p > 0 ? p : (decimal?)null;
		}

		public static string String2Html(this string name, string urlPattern, string style, dynamic identifier)
		{
			return string.IsNullOrEmpty(urlPattern) ?
			    $"<span style=\"{style}\">{name}</span>"
			    :
				string.Format("<a href=\"{2}\" style=\"{0}\">{1}</a>", style, name, string.Format(urlPattern, identifier));
		}

		public static string DateFormat(this DateTime value, string pattern = null)
		{
			var format = string.IsNullOrEmpty(pattern) ? "dd MMM yyyy" : pattern;

			return value.ToString(format, CultureInfo.CurrentCulture);
		}

		public static string Field2Template(this string field)
		{
			return "{{" + field + "}}";
		}

        public static T DictionaryToObject<T>(this IDictionary<string, string> dict) where T : new()
        {
            var t = new T();
            var properties = t.GetType().GetProperties();

            foreach (var property in properties)
            {
                try
                {
                    if (!dict.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    var item = dict.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                    // Find which property type (int, string, double? etc) the CURRENT property is...
                    var tPropertyType = t.GetType().GetProperty(property.Name).PropertyType;

                    // Fix nullables...
                    var newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;

                    // ...and change the type
                    if (string.IsNullOrEmpty(item.Value)) continue;
                    var newA = Convert.ChangeType(item.Value, newT);
                    t.GetType().GetProperty(property.Name).SetValue(t, newA, null);
                }
                catch (Exception)
                {
                    //var error = Utils.FormatError(ex);
                }
            }
            return t;
        }

        //public static T DictionaryToObject<T>(this IDictionary<string, object> source)where T : class, new()
        //{
        //    T someObject = new T();
        //    Type someObjectType = someObject.GetType();

        //    foreach (KeyValuePair<string, object> item in source)
        //    {
        //        someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);
        //    }

        //    return someObject;
        //}

        //public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        //{
        //    return source.GetType().GetProperties(bindingAttr).ToDictionary
        //    (
        //        propInfo => propInfo.Name,
        //        propInfo => propInfo.GetValue(source, null)
        //    );

        //}

        //public static ExpandoObject ToExpandObject(this IDictionary<string, object> dictionary)
        //{
        //    var expando = new ExpandoObject();
        //    var expandoDic = (IDictionary<string, object>)expando;

        //    // go through the items in the dictionary and copy over the key value pairs)
        //    foreach (var kvp in dictionary)
        //    {
        //        // if the value can also be turned into an ExpandoObject, then do it!
        //        if (kvp.Value is IDictionary<string, object>)
        //        {
        //            var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpandObject();
        //            expandoDic.Add(kvp.Key, expandoValue);
        //        }
        //        else if (kvp.Value is ICollection)
        //        {
        //            // iterate through the collection and convert any strin-object dictionaries
        //            // along the way into expando objects
        //            var itemList = new List<object>();
        //            foreach (var item in (ICollection)kvp.Value)
        //            {
        //                if (item is IDictionary<string, object>)
        //                {
        //                    var expandoItem = ((IDictionary<string, object>)item).ToExpandObject();
        //                    itemList.Add(expandoItem);
        //                }
        //                else
        //                {
        //                    itemList.Add(item);
        //                }
        //            }

        //            expandoDic.Add(kvp.Key, itemList);
        //        }
        //        else
        //        {
        //            expandoDic.Add(kvp);
        //        }
        //    }

        //    return expando;
        //}
		#endregion  

		#region videos
		public static string UserId2Tag(this int userId)
		{
			return $"##@{userId}##@";
		}

		public static int? BcTag2UserId(this string tag)
		{
			if (string.IsNullOrEmpty(tag)) return null;
			try
			{
				if (!tag.Contains("##@")) return null;

				var id = tag.Replace("##@", string.Empty);

				int userId;

				var parsed = int.TryParse(id, out userId);

				return parsed ? userId : (int?) null;
			}
			catch (Exception)
			{

				return null;
			}
		}

        public static int Duration2Seconds(this string duration)
        {
            try
            {
                var parts = duration.Split(Convert.ToChar(":"));

                switch (parts.Length)
                {
                    case 1:
                        return int.Parse(parts[0]);
                    case 2:
                        return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
                    case 3:
                        return int.Parse(parts[0]) * 60 * 60 + int.Parse(parts[1]) * 60 + int.Parse(parts[2]);
                    default:
                        return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

		public static long MinSecString2MilliSeconds(this string time)
		{
			try
			{
				var parts = time.Split(Convert.ToChar(":"));

				if (parts.Length != 2) return 0;

				return (Convert.ToInt64(parts[0]) * 60 + Convert.ToInt64(parts[1]))*1000;
			}
			catch (Exception)
			{

				return 0;
			}
		}

		public static TimeSpan? VideoLength2Duration(this long length)
		{
			try
			{
				return TimeSpan.FromSeconds(Math.Round((double)length / 1000, 2));
			}
			catch (Exception)
			{
				return null;
			}
		}
        public static TimeSpan? VideoLengthSeconds2Duration(this long length)
        {
            try
            {
                return TimeSpan.FromSeconds(length);
            }
            catch (Exception)
            {
                return null;
            }
        }

		private static string Duration2MinutesString(this TimeSpan? duration)
		{
			return duration != null
				? $"{duration.Value.Minutes}:{(duration.Value.Seconds < 10 ? "0" + duration.Value.Seconds : duration.Value.Seconds.ToString())}"
			    : string.Empty;
		}

		public static string Duration2HoursString(this TimeSpan? duration)
		{
			if (duration == null) return "00:00";

			return duration.Value.Hours <= 0 ? Duration2MinutesString(duration) :
			    $"{duration.Value.Hours.FormatTimePart()}:{duration.Value.Minutes.FormatTimePart()}:{duration.Value.Seconds.FormatTimePart()}";
		}

		private static string FormatTimePart(this int value)
		{
			return value < 10 ? "0" + value : value.ToString();
		}

		public static string Extension2MimeType(this string extension)
		{
			try
			{
				if (extension == null)
				{
					throw new ArgumentNullException("extension");
				}

				if (!extension.StartsWith("."))
				{
					extension = "." + extension;
				}

				string mime;

				return Constants.MYME_MAPPINGS_DICT.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
			}
			catch (Exception)
			{
				//var error = FormatError(ex);
				return "application/octet-stream";
			}
		}

	    public static string CombimeVideoUrl(this long bcId, int userId,CommonEnums.eVideoPictureTypes type)
	    {
            return
                $"https://courses-videos-prod.beame.io/{userId}/{bcId}/{(type == CommonEnums.eVideoPictureTypes.Still ? Constants.VIDEO_STILL_NAME : Constants.VIDEO_THUMB_NAME)}.jpg";
	    }

        public static string CombimeVideoPictureKey(this long bcId, int userId, CommonEnums.eVideoPictureTypes type)
        {
            return
                $"{userId}/{bcId}/{(type == CommonEnums.eVideoPictureTypes.Still ? Constants.VIDEO_STILL_NAME : Constants.VIDEO_THUMB_NAME)}.jpg";
        }

	   
		#endregion

		#region files

	    public static string User2UploadRefId(this int userId, string uid)
	    {
	        return $"{userId}_{uid}";
	    }
		public static string CombineDocumentPath(this string fileName, int? courseId, int? chapterId)
		{
			if (courseId == null && chapterId == null) return
			    $"{FileEnums.eFileOwners.Course}/Docs/{DateTime.Now.ToShortDateString()}/{fileName}";
			if (courseId == null) return $"{FileEnums.eFileOwners.Course}/Docs/{chapterId}/{fileName}";
			if (chapterId == null) return $"{FileEnums.eFileOwners.Course}/Docs/{courseId}/{fileName}";
			return $"{FileEnums.eFileOwners.Course}/Docs/{courseId}/{chapterId}/{fileName}";
		}

		public static string CombineCourseThumbPath(this string fileName, int? courseId)
		{
			if (courseId == null) return
			    $"{FileEnums.eFileOwners.Course}/Thumbs/{DateTime.Now.ToShortDateString()}/{ShortGuid.NewGuid() + $".{ImageFormat.Jpeg.ToString().ToLower()}"}";
			
			return
			    $"{FileEnums.eFileOwners.Course}/Thumbs/{courseId}/{ShortGuid.NewGuid() + $".{ImageFormat.Jpeg.ToString().ToLower()}"}";
		}
        
		public static string CombineQuizQuestionImagePath(this string fileName, int quizSid)
		{
			return $"{FileEnums.eFileOwners.Quiz}/{quizSid}/Q/{fileName + $".{ImageFormat.Jpeg.ToString().ToLower()}"}";
		}
		public static string CombineQuizQuestionImageUrl(this string fileName, int quizSid)
		{
			return $"{FileEnums.eFileOwners.Quiz}/{quizSid}/Q/{fileName}";
		}

		public static string CombineCertImagePath(this string fileName, int courseId)
		{
			return $"{FileEnums.eFileOwners.Cert}/{courseId}/{fileName + $".{ImageFormat.Png.ToString().ToLower()}"}";
		}
		#endregion
		
		#region dates

		public static DateTime DateTime2Universal(this DateTime date, string ianaTimeZone)
		{
			try
			{
				return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone.IanaToWindows()));
			}
			catch (Exception)
			{
				return date.ToUniversalTime();
			}
		}
		public static string IanaToWindows(this string ianaZoneId)
		{
			var utcZones = new[] { "Etc/UTC", "Etc/UCT" };
			if (utcZones.Contains(ianaZoneId, StringComparer.OrdinalIgnoreCase))
				return "UTC";

			var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

			// resolve any link, since the CLDR doesn't necessarily use canonical IDs
			var links = tzdbSource.CanonicalIdMap
			  .Where(x => x.Value.Equals(ianaZoneId, StringComparison.OrdinalIgnoreCase))
			  .Select(x => x.Key);

			var mappings = tzdbSource.WindowsMapping.MapZones;
			var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(links.Contains));
			if (item == null) return null;
			return item.WindowsId;
		}

		// This will return the "primary" IANA zone that matches the given windows zone.
		// If the primary zone is a link, it then resolves it to the canonical ID.
		public static string WindowsToIana(this string windowsZoneId)
		{
			if (windowsZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
				return "Etc/UTC";

			var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
			var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
			var tzid = tzdbSource.MapTimeZoneId(tzi);
			return tzdbSource.CanonicalIdMap[tzid];
		}
		public static DateTime ToNexMonthFirstDate(this object obj)
		{
			var next = DateTime.Now.AddMonths(1);
			return new DateTime(next.Year, next.Month, 1);
		}

		public static DateTime ToPrevMonthFirstDate(this object obj)
		{
			var prev = DateTime.Now.AddMonths(-1);
			return new DateTime(prev.Year, prev.Month, 1);
		}
		#endregion

		#region quizzes	  
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			var elements = source.ToArray();
			// Note i > 0 to avoid final pointless iteration
			for (var i = elements.Length - 1; i > 0; i--)
			{
				// Swap element "i" with a random earlier element it (or itself)
				var swapIndex = rng.Next(i + 1);
				yield return elements[swapIndex];
				elements[swapIndex] = elements[i];
				// we don't actually perform the swap, we can forget about the
				// swapped element because we already returned it.
			}

			// there is one item remaining that was not returned - we return it now
			yield return elements[0];
		}
		#endregion
	}

	public static class EmailUtilities
	{
		static bool invalid;

		public static bool IsValidEmail(this string strIn)
		{
			invalid = false;
			if (string.IsNullOrEmpty(strIn))
				return false;

			// Use IdnMapping class to convert Unicode domain names. 
			try
			{
				strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,RegexOptions.None, TimeSpan.FromMilliseconds(200));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}

			if (invalid)
				return false;

			// Return true if strIn is in valid e-mail format. 
			try
			{
				return Regex.IsMatch(strIn,
					  @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
					  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
					  RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}

		private static string DomainMapper(this Match match)
		{
			// IdnMapping class with default property values.
			var idn = new IdnMapping();

			string domainName = match.Groups[2].Value;
			try
			{
				domainName = idn.GetAscii(domainName);
			}
			catch (ArgumentException)
			{
				invalid = true;
			}
			return match.Groups[1].Value + domainName;
		}
	}

	public class NameValue
	{
		public string Name { get; set; }
		public object Value { get; set; }
		public string Title { get; set; }
	}
}
