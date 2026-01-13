using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilaDeCampo.ViewModels
{
    public class LoginCongreVM
    {
        [Required(ErrorMessage = "Selecione uma congregação.")]
        [Display(Name = "Congregação")]
        public int CongregacaoId { get; set; }

        [Required(ErrorMessage = "Informe a chave de acesso."),StringLength(50)]
        [DataType(DataType.Password),Display(Name = "Senha")]
        public string ChaveAcesso { get; set; }
        public IEnumerable<SelectListItem> Congregacoes { get; set; } = new List<SelectListItem>();
    }
}
