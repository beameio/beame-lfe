using LFE.Core.Enums;
using LFE.DataTokens;
using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3.Model;

namespace LFE.Application.Services.Interfaces
{
    public interface IAmazonEmailWrapper : IDisposable
    {
        bool SendEmail(long emailId, out string error);
        //bool SendEmail(long emailId,byte[] attachment, out string error);
        bool SendEmailWithAttachment(long emailId, string attachmentPath, out string error);
        bool SendEmailWithAttachment(long emailId, MemoryStream stream, out string error);

    }

    public interface IFacebookServices : IDisposable
    {
        FbUser GetFbUser(long fbUid);
        void SavePostMessage(PostMessageDTO postToken, out string error, long? notificationId = null);
        void CreateUserFbStory(int userId, int courseId, FbEnums.eFbActions action, int? chapterVideoID = null, string additionalMsg = null);
        void SendWaitingPosts();

        string GetUserLongLivedAccessToken(string accessToken, out DateTime? expires);
        FbResponse GetFbUserToken(string accessToken, out string error);
        string GetAccessTokenFromCode(string code, string redirect_url, out string error);
    }


    public interface IFacebookTabAppServices : IDisposable
    {
        
    }

    public interface IS3Wrapper : IDisposable
    {
        string UploadFbImage2S3(string fbImageUrl, string root);
        string Upload(string filename, string contentType, Stream data, out string error);
        string Upload(string filename, string contentType, MemoryStream data, out string error);
        string CopyFromAmazonToTempDirectory(InterfaceFileDTO token, string tempDirectory);
        string GetFileURL(string filename);
        void RemoveFile(string filename);
        bool IsS3FileExists(string key, string bucket);
        GetObjectMetadataResponse GetS3FileMetaData(string key, string bucket);
        bool CreateVideoThumbs(string bucket, string prefix, out string error);
        bool UploadVideoImage(string filename, string contentType, Stream data, out string error);
        bool RemoveVideoFolder(int userId, long bcId, out string error);
    }
   
}
