using LFE.Core.Enums;
using LFE.Model;
using System;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class FileEntityMapper
    {
        public static UserS3FileInterface FileName2FileInterfaceEntity(this string path, int userId, string eTag, string refId, string contentType, long? size, ImportJobsEnums.eFileInterfaceStatus status, string tags)
        {
            return new UserS3FileInterface
                {
                    UserId       = userId
                    ,FilePath    = path
                    ,ETag        = eTag
                    ,BcRefId     = refId
                    ,ContentType = contentType
                    ,Status      = status.ToString()
                    ,FileSize    = size
                    ,Tags        = tags
                    ,AddOn       = DateTime.Now
                };
        }

        public static UserS3FileInterface VideoUploadToken2FileInterfaceEntity(this VideoUploadToken token, int userId,string refId, ImportJobsEnums.eFileInterfaceStatus status)
        {
            return new UserS3FileInterface
                {
                    UserId       = userId
                    ,FilePath    = string.Format("/{0}/{1}/{2}",userId,token.bcId,token.fileName)
                    ,BcRefId     = refId
                    ,ContentType = token.contentType
                    ,Status      = status.ToString()
                    ,FileSize    = token.size
                    ,Tags        = token.tags
                    ,BcIdentifier = token.bcId
                    ,AddOn       = DateTime.Now
                };
        }

        public static void UpdateFileInterfaceEntity(this UserS3FileInterface entity, string title, ImportJobsEnums.eFileInterfaceStatus status)
        {
            entity.Title    = title;
            entity.Status   = status.ToString();
            entity.UpdateOn = DateTime.Now;
        }
    }
}
