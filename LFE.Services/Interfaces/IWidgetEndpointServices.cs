using LFE.DataTokens;
using System;
using System.Collections.Generic;

namespace LFE.Application.Services.Interfaces
{
    public interface IWidgetEndpointServices : IDisposable
    {
        bool SavePluginInstallaltion(PluginInstallationDTO token, out string error);

        bool UninstallPlugin(string uid, out string error);

        List<BaseWebStoreDTO> GetOwnerStores(int id);

        PluginInstallationDTO GetPluginInstallationDto(string uid, out string error);

        bool VerifyPluginOwner(string uid, out string error);
    }
}
