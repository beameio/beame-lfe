using System;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class GeoDtoMapper
    {
        public static CountryDTO Entity2CountryDto(this GEO_CountriesLib entity)
        {
            return new CountryDTO
            {
                CountryId    = entity.CountryId
                ,CountryName = entity.CountryName
                ,A2          = entity.A2
                ,A3          = entity.A3
                ,Index       = entity.OrderIndex
            };
        }

        public static StateDTO Entity2StateDto(this GEO_States entity)
        {
            return new StateDTO
            {
                CountryId    = entity.CountryId
                ,StateId     = entity.StateId
                ,StateName   = entity.StateName
                ,StateCode   = entity.StateCode
            };
        }

        public static BillingAddressViewToken AddressEntity2BillingAddressDto(this USER_BillingAddressToken token)
        {
            return new BillingAddressViewToken
            {
                CountryId         = token.CountryId
                ,StateId          = token.StateId
                ,City             = token.CityName.TrimString()
                ,PostalCode       = token.PostalCode.TrimString()
                ,Street1          = token.Street1.TrimString()
                ,Street2          = token.Street2.TrimString()
                ,AddressId        = token.AddressId
                ,IsDefault        = token.IsDefault
                ,IsActive         = token.IsActive
                ,BillingFirstName = token.FirstName.TrimString()
                ,BillingLastName  = token.LastName.TrimString()
                ,DisplayName      = String.Format("{0} {1} {2} {3} {4}",token.A2 ,token.StateCode ,token.PostalCode.TrimString(), token.FirstName.TrimString(), token.LastName.TrimString())
            };
        }
    }
}
