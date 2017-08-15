using LFE.DataTokens;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class AuthorEntityMapper
    {
        public static DB_CustomEvents Token2Entity(this DashboardEventToken token, int userId)
        {
            return new DB_CustomEvents
            {
                UserId = userId,
                Color = token.Color,
                Date = (DateTime)token.Date,
                Name = token.Name,
                Uid = Guid.NewGuid().ToString()
            };
        }
        
    }
}
