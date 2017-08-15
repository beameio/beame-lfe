using System.IO.Compression;
using System.Web.Mvc;

namespace LFE.Core.Extensions
{
    public class EnableCompressionAttribute : ActionFilterAttribute
    {
        const CompressionMode Compress = CompressionMode.Compress;

        public override void OnActionExecuting( ActionExecutingContext filterContext ) {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            var acceptEncoding = request.Headers["Accept-Encoding"];
            if ( acceptEncoding == null ) return;
            
            if ( acceptEncoding.ToLower().Contains( "gzip" ) ) {
                response.Filter = new GZipStream( response.Filter, Compress );
                response.AppendHeader( "Content-Encoding", "gzip" );
            } else if ( acceptEncoding.ToLower().Contains( "deflate" ) ) {
                response.Filter = new DeflateStream( response.Filter, Compress );
                response.AppendHeader( "Content-Encoding", "deflate" );
            }
        }
    }

}
