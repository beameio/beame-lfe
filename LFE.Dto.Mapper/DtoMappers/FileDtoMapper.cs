using LFE.DataTokens;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class FileDtoMapper
    {
        public static InterfaceFileDTO Entity2InterfaceFileDto(this UserS3FileInterface entity)
        {
            return new InterfaceFileDTO
                {
                    fileId           = entity.FileId
                    ,userId          = entity.UserId
                    ,name            = entity.FilePath
                    ,eTag            = entity.ETag
                    ,contentType     = entity.ContentType
                    ,title           = entity.Title
                    ,tags            = entity.Tags
                };
        }
    }
}
