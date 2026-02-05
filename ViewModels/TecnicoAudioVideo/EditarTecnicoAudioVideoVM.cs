using System.ComponentModel.DataAnnotations;
using FilaDeCampo.Models.Enums;

namespace FilaDeCampo.ViewModels.TecnicoAudioVideo;

public class EditarTecnicoAudioVideoVM
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150)]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "A função permitida é obrigatória")]
    public FuncaoAudioVideo FuncaoPermitida { get; set; }

    public bool Ativo { get; set; }
}