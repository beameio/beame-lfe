using System;
using System.ComponentModel.DataAnnotations;

namespace LFE.Portal.Areas.AuthorAdmin.Models
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private const string DEFAULT_ERROR_MSG_TEMPLATE = "{0} should be after {1}";

        public DateGreaterThanAttribute()
        {
            if (String.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = DEFAULT_ERROR_MSG_TEMPLATE;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var date = (DateTime?) value;
            var otherValue = validationContext.ObjectType.GetProperty(OtherField).GetValue(validationContext.ObjectInstance);
            var otherDate = (DateTime?) otherValue;
            if (date.HasValue && otherDate.HasValue && otherDate > date)
            {
                return new ValidationResult(String.Format(ErrorMessage, validationContext.DisplayName, OtherField));
            }

            return ValidationResult.Success;

        }

        public string OtherField { get; set; }
    }
}