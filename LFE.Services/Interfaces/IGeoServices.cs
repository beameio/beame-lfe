using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IGeoServices : IDisposable
    {
        List<BaseCurrencyDTO> ActiveCurrenciesList { get; }
        List<CountryDTO> ActiveCountries();
        List<StateDTO> States();
        List<StateDTO> GetStates(short countryId);
    }
}
