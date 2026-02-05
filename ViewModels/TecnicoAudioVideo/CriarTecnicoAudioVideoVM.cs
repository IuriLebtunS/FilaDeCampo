using System.ComponentModel.DataAnnotations;
using FilaDeCampo.Models.Enums;

namespace FilaDeCampo.ViewModels.TecnicoAudioVideo;

public class CriarTecnicoAudioVideoVM
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150)]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "A função permitida é obrigatória")]
    public FuncaoAudioVideo FuncaoPermitida { get; set; }
}