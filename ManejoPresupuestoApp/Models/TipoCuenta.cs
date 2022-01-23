using ManejoPresupuestoApp.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Models
{
    public class TipoCuenta //: IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(maximumLength:50,MinimumLength = 3, ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1}")]
        [PrimeraLetraMayuscula]
        [Remote(action: "ExisteTipoCuenta",controller:"TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (string.IsNullOrWhiteSpace(Nombre))
        //        yield return ValidationResult.Success;

        //    char primeraLetra = Nombre.ToString().ElementAt(0);

        //    if (primeraLetra.ToString() != primeraLetra.ToString().ToUpper())
        //        yield return new ValidationResult($"La primera letra de {nameof(Nombre)} debe ser mayúscula",new[] { nameof(Nombre) });
        //    else
        //        yield return ValidationResult.Success;
        //}
    }
}

