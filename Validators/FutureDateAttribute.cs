using System;
using System.ComponentModel.DataAnnotations;

namespace DojoActivityCenter.Validators
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime)
            {
                if ((DateTime)value > DateTime.Now)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Activity Date cannot be in the past");
                }
            }
            else
            {
                return new ValidationResult("Not a valid date");
            }

        }
    }

}