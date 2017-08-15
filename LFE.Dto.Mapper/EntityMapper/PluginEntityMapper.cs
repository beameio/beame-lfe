using System;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class PluginEntityMapper
    {
        public static APP_PluginInstallations Token2PluginInstallationEntity(this PluginInstallationDTO token)
        {
            return new APP_PluginInstallations
            {
                AddOn     = token.AddOn ?? DateTime.Now,
                UId       = token.Uid,
                TypeId    = (byte)token.Type,
                Domain    = token.Domain,
                UserId    = token.UserId,
                IsActive  = token.IsActive,
                CreatedBy = DtoExtensions.CurrentUserId
                //Version = pluginVersion,
            };
        }

        public static void UpdatePluginStatus(this APP_PluginInstallations entity,bool isActive)
        {
            entity.IsActive   = isActive;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }

        public static void UpdatePluginDomain(this APP_PluginInstallations entity, string url)
        {
            entity.Domain     = url;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
    }
}
