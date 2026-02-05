using FilaDeCampo.Models.Enums;

namespace FilaDeCampo.Models;

public class TecnicoAudioVideo
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public FuncaoAudioVideo FuncaoPermitida { get; set; }

    public bool Ativo { get; set; } = true;

    public int OrdemRodizio { get; set; }

    public int CongregacaoId { get; set; }
}