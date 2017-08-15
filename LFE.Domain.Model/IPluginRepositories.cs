using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface IPluginInstallationsRepository : IRepository<APP_PluginInstallations> { }

    public interface IPluginInstallationStoresRepository : IRepository<APP_PluginInstallationStores> { }
}
