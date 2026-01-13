using System.ComponentModel.DataAnnotations;

namespace FilaDeCampo.ViewModels
{
    public class CriarCongreVM
    {
        [Required(ErrorMessage = "O nome da congregação é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A chave de acesso é obrigatória.")]
        [StringLength(50)]
        public string ChaveAcesso { get; set; }

        public bool Ativa { get; set; } = true;
    }
}