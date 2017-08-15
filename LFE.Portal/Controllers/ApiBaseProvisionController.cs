using System;
using LFE.Portal.Interfaces;

namespace LFE.Portal.Controllers
{
    public class ApiBaseProvisionController : ApiBaseController,IApiInterfaces
    {
        /// <summary>
        /// ApplicationUid for request
        /// </summary>
        public Guid AppId { get; set; }


        /// <summary>
        /// Requested record Uid route value
        /// </summary>
        public Guid Uid { get; set; }
    }
}
