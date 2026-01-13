using System.ComponentModel.DataAnnotations;

namespace FilaDeCampo.ViewModels
{
    public class LoginMasterVM
    {
        [Required(ErrorMessage = "Informe o usuário.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}