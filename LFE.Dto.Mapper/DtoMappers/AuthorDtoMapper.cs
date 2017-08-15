using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class AuthorDtoMapper
    {
    
        public static UserVideoDto InterfacedVideo2VideoDTO(this UserS3FileInterface video, int userId, string userTag)
        {
           // var userTagList = new List<string> {userTag};

            return new UserVideoDto
            {
                userId     = userId
               ,identifier = "-1"
               ,fileId     = video.FileId
               ,title      = video.Title ?? video.FilePath
               ,addon      = video.AddOn
               ,minutes    = string.Empty
               ,status     = ImportJobsEnums.eFileInterfaceStatus.Waiting
            };
           
        }

       

        public static BaseListDTO AuthorEntity2BaseListDto(this USER_AuthorWithCourseCountToken entity)
        {
            var token =  new BaseListDTO
            {
                id = entity.Id
                ,name = entity.Entity2FullName()
            };

            if (entity.CoursesCnt == 0 && entity.BundlesCnt == 0) return token;

            token.name = token.name.NameWithContentCounts2DisplayName(entity.CoursesCnt, entity.BundlesCnt);

            return token;
        }

        public static DashboardEventToken Entity2Token(this DB_CustomEvents entity)
        {
            return new DashboardEventToken
            {
                Color = entity.Color,
                Date = entity.Date,
                Enabled = true,
                IsStatic = false,
                Name = entity.Name,
                Type = DashboardEnums.eDbEventTypes.Custom,
                Uid = Guid.Parse(entity.Uid)
            };
        }

        
    }
}
