using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Application.Services
{
    public class WidgetEndpointServices : ServiceBase, IWidgetEndpointServices
    {
        public bool SavePluginInstallaltion(PluginInstallationDTO token,out string error)
        {
            error = string.Empty;

            var entity = PluginInstallationsRepository.Get(x => x.UId == token.Uid && x.IsActive);

            if (entity != null) return true;

            entity = token.Token2PluginInstallationEntity();
            entity.IsActive = true;

            PluginInstallationsRepository.Add(entity);

            return PluginInstallationsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        }

        public bool UninstallPlugin(string uid, out string error)
        {
            var entity = PluginInstallationsRepository.Get(x => x.UId == uid && x.IsActive);
            
            if (entity == null)
            {
                error = "entity not found";
                return false;
            }

            entity.UpdatePluginStatus(false);
            
            PluginInstallationsRepository.Update(entity);
            return PluginInstallationsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        }

        public List<BaseWebStoreDTO> GetOwnerStores(int id)
        {
            var stores = _GetOwnerStores(id).Where(x => x.Status != WebStoreEnums.StoreStatus.Deleted).Select(x => x.WebStoreGridToken2BaseWebStoreDto()).ToList();
            return stores;
        }

        public PluginInstallationDTO GetPluginInstallationDto(string uid, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = PluginInstallationsRepository.Get(x => x.UId == uid && x.IsActive);

                if (entity != null) return entity.Entity2PluginInstallationDto();

                error = "entity not found";
                return null;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("Get Plugin dto::" + uid,ex,CommonEnums.LoggerObjectTypes.Plugin);
                return null;
            }
        }

        public bool VerifyPluginOwner(string uid, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = PluginInstallationsRepository.Get(x => x.UId == uid && x.IsActive);

                if (entity == null)
                {
                    error = "Plugin installation record missing";
                    return false;
                }

                if (entity.UserId == null)
                {
                    entity.UserId     = CurrentUserId;
                    entity.UpdateDate = DateTime.Now;

                    return PluginInstallationsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }

                if (entity.UserId == CurrentUserId) return true;

                error = "You are attempting to login with an LFE account that is not connected to this application. Click here to connect with the LFE account associated with this plugin";
                return false;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("Save Plugin owner::" + uid, ex, CommonEnums.LoggerObjectTypes.Plugin);
                return false;
            }
        }
        
    }
}
