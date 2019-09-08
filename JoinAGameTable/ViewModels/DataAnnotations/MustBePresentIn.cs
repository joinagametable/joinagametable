using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace JoinAGameTable.ViewModels.DataAnnotations
{
    /// <summary>
    /// Validate value against another property's value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple =
        false)]
    public class MustBePresentIn : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public MustBePresentIn(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
            {
                throw new ArgumentException("Property with this name not found");
            }

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);
            var localizer = (IStringLocalizer) validationContext.GetService(typeof(IStringLocalizer<MustBePresentIn>));

            switch (comparisonValue)
            {
                case List<object> dataListObj:
                    return dataListObj.Contains(value)
                        ? ValidationResult.Success
                        : new ValidationResult(localizer?[ErrorMessageString]);
                case List<SelectListItem> dataListSelListItem:
                    return dataListSelListItem.Any(item => item.Value.Equals(value))
                        ? ValidationResult.Success
                        : new ValidationResult(localizer?[ErrorMessageString]);
            }

            return ValidationResult.Success;
        }
    }
}
