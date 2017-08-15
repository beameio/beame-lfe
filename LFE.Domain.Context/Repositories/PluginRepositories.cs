using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class PluginInstallationsRepository : Repository<APP_PluginInstallations>, IPluginInstallationsRepository
    {
        public PluginInstallationsRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }
    }

    public class PluginInstallationStoresRepository : Repository<APP_PluginInstallationStores>, IPluginInstallationStoresRepository
    {
        public PluginInstallationStoresRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }
    }
}
