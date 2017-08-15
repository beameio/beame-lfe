using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using LFE.Core.Enums;
using NLog;
using WebMatrix.WebData;

namespace LFE.Infrastructure.NLogger {
	public class NLogLogger:ILogger {

	   

		private readonly Logger _logger;

		public NLogLogger() {
			_logger = LogManager.GetCurrentClassLogger();           
		}

		private void WriteLog(LogEventInfo theEvent)
		{

			theEvent.Properties["IPAddress"] = GetVisitorIPAddress();
			theEvent.Properties["HostName"]  = GetReferrer();
			theEvent.Properties["UserId"]    = CurrentUserId();
            theEvent.Properties["SessionId"] = CurrentSessionId();
            
			_logger.Log(theEvent);
		}

		public void Info(string message) {
			var theEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
			WriteLog(theEvent);
		}

		public void Info(string message, Guid recordId, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
			theEvent.Properties["RecordGuidId"] = recordId;
			theEvent.Properties["objectType"] = objectType.ToString();
			WriteLog(theEvent);    
		}

		public void Info(string message, long recordId, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
			theEvent.Properties["RecordIntId"] = recordId;
			theEvent.Properties["objectType"] = objectType.ToString();
			WriteLog(theEvent);    
		}

		 

		public void Warn(string message) {
			var theEvent = new LogEventInfo(LogLevel.Warn, _logger.Name, message);
			WriteLog(theEvent);
		}
        public void Warn(string message, CommonEnums.LoggerObjectTypes objectType)
        {
            var theEvent = new LogEventInfo(LogLevel.Warn, _logger.Name, message);
            theEvent.Properties["objectType"] = objectType.ToString();
            WriteLog(theEvent);
        }
		public void Debug(string message, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent = new LogEventInfo(LogLevel.Debug, _logger.Name, message);
			theEvent.Properties["objectType"] = objectType.ToString();
			WriteLog(theEvent);
		}

		public void Debug(string message)
		{
			var theEvent = new LogEventInfo(LogLevel.Debug, _logger.Name, message);
			WriteLog(theEvent);
		}

		public void Error(string message) {

			var theEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			WriteLog(theEvent);
		}
	   
		public void Fatal(string message) {

			var theEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			WriteLog(theEvent);
		}

		public void Fatal(Exception x) {
			var theEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, LogUtility.BuildExceptionMessage(x));
			WriteLog(theEvent);
		}

		public void Fatal(string message, Exception x)
		{
			_logger.FatalException(message,x);
		}

		public void Error(string message, Guid recordId, CommonEnums.LoggerObjectTypes objectType)
		{   
			var theEvent                            = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			theEvent.Properties["Origin"]           = stackTraceCaller();
			theEvent.Properties["RecordGuidId"]     = recordId;
			theEvent.Properties["objectType"] = objectType.ToString();
			WriteLog(theEvent);
		}

		public void Error(string message,Exception ex, int? recordId, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent                            = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			theEvent.Properties["Origin"]           = stackTraceCaller();
			theEvent.Properties["RecordIntId"]      = recordId;
			theEvent.Properties["objectType"] = objectType.ToString();
			var e = getImportantException(ex);
			theEvent.Exception = e;
			WriteLog(theEvent);
		}

		public void Error(string message, Exception ex, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			theEvent.Properties["Origin"] = stackTraceCaller();
			theEvent.Properties["objectType"] = objectType.ToString();
			var e = getImportantException(ex);
			theEvent.Exception = e;
			WriteLog(theEvent);
		}

		public void Error(string message, Guid? recordId, Exception ex, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent                            = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			theEvent.Properties["Origin"]           = stackTraceCaller();
			theEvent.Properties["RecordGuidId"]     = recordId;
			theEvent.Properties["objectType"]       = objectType.ToString();
			var e                                   = getImportantException(ex);
			theEvent.Exception                      = e;
			WriteLog(theEvent);
		}
		public void Error(string message, long recordId, Exception ex, CommonEnums.LoggerObjectTypes objectType)
		{
			var theEvent                            = new LogEventInfo(LogLevel.Error, _logger.Name, message);
			theEvent.Properties["Origin"]           = stackTraceCaller();
			theEvent.Properties["RecordIntId"]      = recordId;
			theEvent.Properties["objectType"]       = objectType.ToString();
			var e                                   = getImportantException(ex);
			theEvent.Exception                      = e;

			WriteLog(theEvent);
		}


		private Exception getImportantException(Exception ex)
		{
			return ex.InnerException ?? ex;
		}
		public void Error(Exception x)
		{
			Error(LogUtility.BuildExceptionMessage(x));
		}
		private static string stackTraceCaller()
		{
			/* call the main method using 3rd entry in frames (first entry is this stackTraceCaller method 2nd is the log method )*/
			var stackFrames = new StackTrace().GetFrames();
			if (stackFrames != null)
			{
				var stackFrame = stackFrames[2];
				var method     = stackFrame.GetMethod();
				var methodName = method.Name;
				if (method.ReflectedType != null)
				{
					var className  = method.ReflectedType.Name;
					return className + " " + methodName;
				}
			}

			return string.Empty;
		}

		#region web events
		private static int? CurrentUserId()
		{
			try
			{
				return !WebSecurity.IsAuthenticated ? (int?)null : Convert.ToInt32(GetClaim(ClaimTypes.NameIdentifier).Value);
			}
			catch (Exception)
			{
				return null;
			}
		}

	    private string CurrentSessionId()
	    {
	        try
	        {
	            return HttpContext.Current.Session.SessionID;
	        }
	        catch (Exception)
	        {
	            return null;
	        }
	    }
		private static Claim GetClaim(string type)
		{
			return ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == type);
		}
		private static string GetReferrer()
		{
			try
			{
				if (HttpContext.Current == null) return "out of http context";
				var cookie = HttpContext.Current.Request.Cookies["_lfeReferrer"];
				var referrer = cookie != null ? HttpUtility.UrlDecode(cookie.Value) : "unknown";
				return referrer;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		private static string GetVisitorIPAddress(bool GetLan = false)
		{
			try
			{
				var visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

				if (String.IsNullOrEmpty(visitorIPAddress))
					visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

				if (String.IsNullOrEmpty(visitorIPAddress))
					visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

				if (String.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
				{
					GetLan = true;
					visitorIPAddress = String.Empty;
				}

				if (!GetLan) return visitorIPAddress;

				if (!String.IsNullOrEmpty(visitorIPAddress)) return visitorIPAddress;

				//This is for Local(LAN) Connected ID Address
				var stringHostName = Dns.GetHostName();
				//Get Ip Host Entry
				var ipHostEntries = Dns.GetHostEntry(stringHostName);
				//Get Ip Address From The Ip Host Entry Address List
				var arrIpAddress = ipHostEntries.AddressList;

				try
				{
					visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
				}
				catch
				{
					try
					{
						visitorIPAddress = arrIpAddress[0].ToString();
					}
					catch
					{
						try
						{
							arrIpAddress = Dns.GetHostAddresses(stringHostName);
							visitorIPAddress = arrIpAddress[0].ToString();
						}
						catch
						{
							visitorIPAddress = "127.0.0.1";
						}
					}
				}
				return visitorIPAddress;
			}
			catch (Exception)
			{
				return String.Empty;
			}
		}

		#endregion
	}
}   