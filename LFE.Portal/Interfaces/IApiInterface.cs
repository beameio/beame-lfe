using System;

namespace LFE.Portal.Interfaces
{
    public interface IApiInterfaces
    {
        /// <summary>
        /// Application Uid for request
        /// </summary>
        Guid AppId { get; set; }

        /// <summary>
        /// Requested record Uid route value
        /// </summary>
        Guid Uid { get; set; }

    }
}