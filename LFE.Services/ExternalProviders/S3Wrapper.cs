using Amazon.S3;
using Amazon.S3.Model;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.IO;
using System.Net;
using Amazon;

namespace LFE.Application.Services.ExternalProviders
{
    public class S3Wrapper : ServiceBase, IS3Wrapper
    {
        #region parameters
        
        #endregion

        #region properties
        private static string AccessKeyId { get; set; }
        private static string Secret { get; set; }
        private static string BucketName { get; set; }
        private static string VideoBucketName { get; set; }
        private static AmazonS3Client s3Client { get; set; }        
        #endregion

        #region .ctor
        public S3Wrapper()
        {
            AccessKeyId     = Utils.GetKeyValue("S3AccessKeyId"); 
            Secret          = Utils.GetKeyValue("S3Secret");
            BucketName      = Utils.GetKeyValue("S3BucketName");
            VideoBucketName = Utils.GetKeyValue("S3VideoBucketName");

            s3Client        = new AmazonS3Client(AccessKeyId, Secret, RegionEndpoint.USEast1);
        }
        #endregion

        #region helpers
        private Stream GetImageStreamFromUrl(string url)
        {
            WebResponse result = null;
            //Image rImage = null;
            try
            {
                var request = WebRequest.Create(url);
                result = request.GetResponse();
                var stream = result.GetResponseStream();
                if (stream != null)
                {
                    var br = new BinaryReader(stream);
                    var rBytes = br.ReadBytes(1000000);
                    br.Close();
                    result.Close();
                    var imageStream = new MemoryStream(rBytes, 0, rBytes.Length);
                    return imageStream;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetImageStreamFromUrl",ex,CommonEnums.LoggerObjectTypes.S3Wrapper);
            }
            finally
            {
                result?.Close();
            }

            return null;

        }
        #endregion

        #region public services
        public string UploadFbImage2S3(string fbImageUrl, string root)
        {
            var fileName = root + ShortGuid.NewGuid() + Path.GetExtension(fbImageUrl);
            var strm = GetImageStreamFromUrl(fbImageUrl);
            if (strm == null) return string.Empty;
            string error;
            var s = Upload(fileName, "image/" + Path.GetExtension(fbImageUrl), strm, out error);

            return !string.IsNullOrEmpty(s) ? fileName : string.Empty;
        }

        public string Upload(string filename, string contentType, Stream data,out string error)
        {
            error = null;
            try
            {
                    var req = new PutObjectRequest
                    {
                        Key          = filename
                        ,BucketName  = BucketName
                        ,ContentType = contentType
                        ,CannedACL   = S3CannedACL.PublicRead
                        ,InputStream = data
                    };
                
                    Logger.Debug("Starting upload file " + filename + " to S3 from stream  on " + DateTime.Now, CommonEnums.LoggerObjectTypes.S3Wrapper);
                    var response = s3Client.PutObject(req);
                    Logger.Debug("Finish upload file " + filename + " to S3 from stream  on " + DateTime.Now, CommonEnums.LoggerObjectTypes.S3Wrapper);
                    return response.ETag;                                        
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("upload file " + filename +" to S3",null,ex,CommonEnums.LoggerObjectTypes.S3Wrapper);
                return string.Empty;
            }
        }
        
        public string Upload(string filename, string contentType, MemoryStream data,out string error)
        {
            error = null;
            try
            {
                    var req = new PutObjectRequest
                    {
                        Key          = filename
                        ,BucketName  = BucketName
                        ,ContentType = contentType
                        ,CannedACL   = S3CannedACL.PublicRead
                        ,InputStream = data
                    };
                
                    Logger.Debug("Starting upload file " + filename + " to S3 from stream  on " + DateTime.Now, CommonEnums.LoggerObjectTypes.S3Wrapper);
                    var response = s3Client.PutObject(req);
                    Logger.Debug("Finish upload file " + filename + " to S3 from stream  on " + DateTime.Now, CommonEnums.LoggerObjectTypes.S3Wrapper);
                    return response.ETag;                                        
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("upload file " + filename +" to S3",null,ex,CommonEnums.LoggerObjectTypes.S3Wrapper);
                return string.Empty;
            }
        }

        public string ToS3SignedUrl(string url,string bucketName)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key        = url.Replace(bucketName, "").Replace(S3_ROOT_URL, "").TrimStart('/'),
                    Expires    = DateTime.Now.AddDays(1)
                };

                var signedUrl = s3Client.GetPreSignedURL(request);

                return signedUrl;
            }
            catch (Exception ex)
            {
                Logger.Error("get S3 signed url",ex,CommonEnums.LoggerObjectTypes.S3Wrapper);
                return url;
            }
        }
        public string CopyFromAmazonToTempDirectory(InterfaceFileDTO token, string tempDirectory)
        {
            try
            {
                //create get object request token
                var req = new GetObjectRequest
                {
                    EtagToMatch = token.eTag
                    ,BucketName = BucketName
                    ,Key = token.name
                };

                //set temp location to download file from S3                
                var url = tempDirectory + token.name;

                //save file to temp location
                s3Client.GetObject(req).WriteResponseStreamToFile(url);

                //return temp file url if exists == downloaded from S3
                return File.Exists(url) ? url : string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("copy file " + token.name + " from S3 to temp directory", token.fileId , ex, CommonEnums.LoggerObjectTypes.S3Wrapper);
                return string.Empty;
            }
        }

        public  string GetFileURL(string filename)
        {
            return $"{S3_ROOT_URL}{BucketName}/{filename}";
           
        }        
      
        public void RemoveFile(string filename)
        {
            try
            {
                var req = new DeleteObjectRequest
                {
                    Key        = filename,
                    BucketName = BucketName
                };

                s3Client.DeleteObject(req);

                Logger.Debug("file " + filename + " removed from S3 on " + DateTime.Now,CommonEnums.LoggerObjectTypes.S3Wrapper);
            }
            catch (Exception ex)
            {                
                Logger.Error("remove file " + filename + " from S3", null, ex, CommonEnums.LoggerObjectTypes.S3Wrapper);
            }
        }

        public bool UploadVideoImage(string filename, string contentType, Stream data, out string error)
        {
            error = null;
            try
            {
                 var req = new PutObjectRequest
                    {
                        Key          = filename
                        ,BucketName  = VideoBucketName
                        ,ContentType = contentType
                        ,CannedACL   = S3CannedACL.PublicRead
                        ,InputStream = data
                    };
                var response = s3Client.PutObject(req);
                return !string.IsNullOrEmpty(response.ETag);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("upload file " + filename + " to S3", null, ex, CommonEnums.LoggerObjectTypes.S3Wrapper);
                return false;
            }
        }

        public bool IsS3FileExists(string key,string bucket)
        {
            return GetS3FileMetaData(key, bucket) != null;
        }

        public GetObjectMetadataResponse GetS3FileMetaData(string key, string bucket)
        {

            try
            {
                var request = new GetObjectMetadataRequest
                {
                     Key        = key
                    ,BucketName = bucket
                };
                var response = s3Client.GetObjectMetadata(request);
                return response;
            }
            catch (Exception )
            {
                return null;
            }
        }

        private void _createVideoThumbs(string bucket, string prefix,string typePrefix)
        {
            var i = 2;

            while (i >= 1)
            {
                var srcKey = $"{prefix}{typePrefix}-0000{i}.jpg";
                var destKey = $"{prefix}{typePrefix}.jpg";

                if (IsS3FileExists(srcKey, bucket))
                {
                    var copyRequest = new CopyObjectRequest
                    {
                        SourceBucket      = bucket,
                        SourceKey         = srcKey,
                        DestinationBucket = bucket,
                        DestinationKey    = destKey,
                        CannedACL         = S3CannedACL.PublicRead
                    };
                    s3Client.CopyObject(copyRequest);
                    Logger.Info(destKey + " thumb created");
                    return;
                }

                i--;
            }
        }

        public bool CreateVideoThumbs(string bucket,string prefix,out string error)
        {
            error = string.Empty;
            
            try
            {
                _createVideoThumbs(bucket, prefix, "thumbnail");
                _createVideoThumbs(bucket, prefix, "still");

                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("create thumbs for video" + prefix , null, ex, CommonEnums.LoggerObjectTypes.S3Wrapper);
                return false;
            }
        }

        public bool RemoveVideoFolder(int userId,long bcId,out string error)
        {
            error = string.Empty;
            try
            {
                var key = $"{userId}/{bcId}";
                var request = new ListObjectsRequest
                {
                    BucketName = VideoBucketName,
                    Prefix = key
                };
                
                var response = s3Client.ListObjects(request);


                DeleteObjectRequest req;
                foreach (var o in response.S3Objects)
                {
                    req = new DeleteObjectRequest
                    {
                        BucketName = VideoBucketName,
                        Key = o.Key
                    };

                    s3Client.DeleteObject(req);
                }

                req = new DeleteObjectRequest
                {
                    BucketName = VideoBucketName,
                    Key = key
                };
                

                s3Client.DeleteObject(req);

                Logger.Debug("video " + bcId + " removed from S3 on " + DateTime.Now, CommonEnums.LoggerObjectTypes.S3Wrapper);
                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("remove video" + bcId + " from S3", null, ex, CommonEnums.LoggerObjectTypes.S3Wrapper);
                return false;
            }
        }
        #endregion
    }
}
