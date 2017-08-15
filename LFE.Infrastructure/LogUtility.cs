using System;
using System.Web;

namespace LFE.Infrastructure.NLogger {
	public class LogUtility {

		public static string BuildExceptionMessage(Exception x) {
			
			Exception logException=x;
			if(x.InnerException!=null)
				logException=x.InnerException;

			string strErrorMsg= Environment.NewLine+"Error in Path :" + HttpContext.Current.Request.Path;

			// Get the QueryString along with the Virtual Path
			strErrorMsg += Environment.NewLine + "Raw Url :" + HttpContext.Current.Request.RawUrl;

			
			// Get the error message
			strErrorMsg += Environment.NewLine + "Message :" + logException.Message;

			// Source of the message
			strErrorMsg += Environment.NewLine + "Source :" + logException.Source;

			// Stack Trace of the error

			strErrorMsg += Environment.NewLine + "Stack Trace :" + logException.StackTrace;

			// Method where the error occurred
			strErrorMsg += Environment.NewLine + "TargetSite :" + logException.TargetSite;
			return strErrorMsg;
		}
	}
}
