using System;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using System.Collections.Generic;
using System.Linq;
using LFE.Model;


namespace LFE.Application.Services
{
    public class GeoServices : ServiceBase, IGeoServices
    {
        private static List<CountryDTO> _activeCountries = new List<CountryDTO>();
        private static List<StateDTO> _states = new List<StateDTO>();
        private static List<BaseCurrencyDTO> _activeCurrencies = new List<BaseCurrencyDTO>();

        public List<BaseCurrencyDTO> ActiveCurrenciesList
        {
            get { return _activeCurrencies; } 
        }

        public GeoServices()
        {
            if(_activeCountries.Any()) return;
            _activeCurrencies = ActiveCurrencies.Select(x=>x.ToBaseCurrencyDto()).ToList();
        }

        public List<CountryDTO> ActiveCountries()
        {
           
            if (_activeCountries.Any()) return _activeCountries;

            try
            {
                //_activeCountries = GeoCountriesRepository.GetMany(x => x.IsActive).Select(x => x.Entity2CountryDto()).ToList();

                //return _activeCountries;

                using (var context = new lfeAuthorEntities())
                {
                    _activeCountries = context.GEO_CountriesLib.Where(x => x.IsActive).ToList().Select(x => x.Entity2CountryDto()).ToList();

                    return _activeCountries;
                }
            }
            catch (Exception ex)
            {
                
                Logger.Error("get countries",ex,CommonEnums.LoggerObjectTypes.Geo);

                //using (var context = new lfeAuthorEntities())
                //{
                //    _activeCountries = context.GEO_CountriesLib.Where(x => x.IsActive).ToList().Select(x => x.Entity2CountryDto()).ToList();

                //    return _activeCountries;
                //}

                return new List<CountryDTO>();
            }
           
        }

        public List<StateDTO> States()
        {
            if (_states.Any()) return _states;

            try
            {
               
                _states = GeoStatesRepository.GetAll().Select(x => x.Entity2StateDto()).ToList();
                
                return _states;    
            }
            catch (Exception ex)
            {

                Logger.Error("get states", ex, CommonEnums.LoggerObjectTypes.Geo);

                using (var context = new lfeAuthorEntities())
                {
                    _states = context.GEO_States.Select(x => x.Entity2StateDto()).ToList();

                    return _states;
                }
            }
        }

        public List<StateDTO> GetStates(short countryId)
        {
            try
            {
                return States().Where(x => x.CountryId == countryId).ToList();

            }
            catch (Exception ex)
            {
                Logger.Error("GetStates", null, ex,CommonEnums.LoggerObjectTypes.Geo);
                return new List<StateDTO>();
            }
        }
    }
}
