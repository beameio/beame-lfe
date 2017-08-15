using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Portal.Controllers
{
    public class GeoController : Controller
    {
        private readonly IGeoServices _geoServices;

        public GeoController()
        {
            if (_geoServices == null) _geoServices = DependencyResolver.Current.GetService<IGeoServices>();
        }

        public JsonResult GetCountryStates(int? countryId)
        {
            var states = new List<StateDTO>();

            if (countryId != null)
            {
                states = _geoServices.GetStates((short)countryId).ToList();
            }

            return Json(states, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsStateIdRequired(int? CountryId, int? StateId)
        {
            var isValid = !(CountryId != null && Constants.COUNTRIES_WITH_STATES.Contains((int)CountryId) && StateId == null);

            return Json(isValid, JsonRequestBehavior.AllowGet);
        }

    }
}
