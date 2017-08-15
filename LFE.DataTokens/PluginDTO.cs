using System;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class PluginInstallationDTO
    {
        public PluginInstallationDTO()
        {
            IsActive = true;
        }

        public int InstallationId { get; set; }
        public string Uid { get; set; }
        public int? UserId { get; set; }
        public PluginEnums.ePluginType Type { get; set; }
        public string Domain { get; set; }

        public DateTime? AddOn { get; set; }

        public bool IsActive { get; set; }
    }

    public class PluginInstallationStoresDTO
    {
        public int RowId { get; set; }
        public int StoreId { get; set; }
        public int InstallationId { get; set; }
        public string PageUrl { get; set; }
    }

}
