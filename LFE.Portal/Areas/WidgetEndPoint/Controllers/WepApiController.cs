using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.WidgetEndPoint.Controllers
{
    public class WepApiController : ApiController
    {
        private readonly IWidgetEndpointServices _widgetWebStoreServices;

        public WepApiController()
        {
            _widgetWebStoreServices = DependencyResolver.Current.GetService<IWidgetEndpointServices>();
        }

    }
}
