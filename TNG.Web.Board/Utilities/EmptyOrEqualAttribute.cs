using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Utilities
{
    public class EmptyOrEqualAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public EmptyOrEqualAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;

            if (string.IsNullOrEmpty(currentValue))
                return ValidationResult.Success!;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (string)property.GetValue(validationContext.ObjectInstance);

            if (!currentValue.Equals(comparisonValue))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success!;
        }
    }
}
