using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Validations
{
    public class PrimeraLetraMayusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            char primeraLetra = value.ToString().ElementAt(0);

            if (primeraLetra.ToString() != primeraLetra.ToString().ToUpper())
                return new ValidationResult("La primera letra debe ser mayúscula");
            else
                return ValidationResult.Success;
        }
    }
}
