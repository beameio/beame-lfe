using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace LFE.Portal.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiModels
    {
        /// <summary>
        /// 
        /// </summary>
        public class ApiMessageError
        {
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool isCallbackError { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> errors { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public ApiMessageError(): this(string.Empty)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public ApiMessageError(string errorMessage)
            {
                isCallbackError = true;
                errors = new List<string>();
                message = errorMessage;
            }

            /// <summary>
            /// 
            /// </summary>
            public ApiMessageError(ModelStateDictionary modelState)
            {
                isCallbackError = true;
                errors = new List<string>();
                message = "Model is invalid.";

                // add errors into our client error model for client
                foreach (var modelItem in modelState)
                {
                    var modelError = modelItem.Value.Errors.FirstOrDefault();
                    if (!string.IsNullOrEmpty(modelError?.ErrorMessage))
                        errors.Add(modelItem.Key + ": " +
                                    ParseModelStateErrorMessage(modelError.ErrorMessage));
                    else
                        errors.Add(modelItem.Key + ": " +
                                    ParseModelStateErrorMessage(modelError?.Exception.Message));
                }
            }

            /// <summary>
            /// Strips off anything after period - line number etc. info
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            string ParseModelStateErrorMessage(string msg)
            {
                var period = msg.IndexOf('.');
                if (period < 0 || period > msg.Length - 1)
                    return msg;

                // strip off 
                return msg.Substring(0, period);
            }
        }
    }
}
