using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LFE.DataTokens
{
    public class CountryDTO
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public int? Index { get; set; }

    }

    public class StateDTO
    {
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }     
    }

    public class AddressDTO
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Street Line 1")]
        public string Street1 { get; set; }

        [Display(Name = "Street Line 2")]
        public string Street2 { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        //[Required(ErrorMessage = "*")]
        //[Remote("IsStateIdRequired", "Geo", AdditionalFields = "CountryId")]
        public short? StateId { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Country")]
        public short? CountryId { get; set; }

        public string CountryCode { get; set; }
        public string StateCode { get; set; }

        [Required(ErrorMessage = "*")]
        [DataType(DataType.PostalCode, ErrorMessage = ("!*"))]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }        
    }

    public class BillingAddressDTO : AddressDTO
    {
        public BillingAddressDTO()
        {
            AddressId = -1;
            IsDefault = true;
            IsActive  = true;
        }
        public int UserId { get; set; }

        public int AddressId { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "First Name")]
        public string BillingFirstName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Last Name")]
        public string BillingLastName { get; set; }
    }

    public class BillingAddressViewToken : BillingAddressDTO
    {
        public string DisplayName { get; set; }
    }
}
