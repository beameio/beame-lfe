using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class PluginDtoMapper
    {
        public static PluginRepDTO Entity2PluginRepDTO(this APP_PluginsInstallationsRep entity)
        {
            var type = Utils.ParseEnum<PluginEnums.ePluginType>(entity.TypeId);

            return new PluginRepDTO
            {
                AddOn          = entity.AddOn,
                Domain         = entity.Domain,
                InstallationId = entity.InstallationId,
                IsActive       = entity.IsActive,
                Type           = type,
                TypeName       = type.ToString(),
                UId            = entity.UId,
                UpdateDate     = entity.UpdateDate,
                UserAddOn      = entity.UserAddOn,
                User           = entity.UserId == null ? new BaseUserInfoDTO()
                                                        : new BaseUserInfoDTO {
                                                                                UserId = (int)entity.UserId, 
                                                                                Email     = entity.Email, 
                                                                                FullName  = entity.Entity2FullName() }
            };
        }

        public static PluginInstallationDTO Entity2PluginInstallationDto(this APP_PluginInstallations entity)
        {
            return new PluginInstallationDTO
            {
                InstallationId = entity.InstallationId
                ,UserId        = entity.UserId
                ,Uid           = entity.UId
                ,Domain        = entity.Domain
                ,Type          = (PluginEnums.ePluginType)entity.TypeId
            };
        }

        public static PluginInstallationStoresDTO Entity2PluginInstallationStoresDto(this APP_PluginInstallationStores entity)
        {
            return new PluginInstallationStoresDTO
            {
                InstallationId = entity.InstallationId
                ,PageUrl       = entity.PageUrl
                ,RowId         = entity.RowId
                ,StoreId       = entity.StoreId
            };
        }
    }
}
