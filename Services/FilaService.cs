using FilaDeCampo.Data;
using FilaDeCampo.Models;
using Microsoft.EntityFrameworkCore;

namespace FilaDeCampo.Services;

public class FilaService
{
    private readonly DbSolaresCampo _context;
    public FilaService(DbSolaresCampo context)
    {
        _context = context;
    }

    public List<EscalaDeSabado> GerarFila(
        int mes,
        int ano,
        List<Dirigente> dirigentes,
        int ultimoDirigenteId)

    {
        var resultado = new List<EscalaDeSabado>();

        var data = new DateTime(ano, mes, 1);
        var sabados = new List<DateTime>();

        while (data.Month == mes)
        {
            if (data.DayOfWeek == DayOfWeek.Saturday)
                sabados.Add(data);

            data = data.AddDays(1);
        }

        // garante ordem do rodízio
        var ordenados = dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.OrdemRodizio)
            .ToList();

        int index = ordenados.FindIndex(d => d.Id == ultimoDirigenteId);
        index = index < 0 ? 0 : index + 1;

        for (int i = 0; i < sabados.Count; i++)
        {
            var dirigente = ordenados[index % ordenados.Count];

            resultado.Add(new EscalaDeSabado
            {
                Data = sabados[i],
                DirigenteId = dirigente.Id,
                Dirigente = dirigente,
                Observacao = i == sabados.Count - 1
                    ? "Testemunho no comércio"
                    : null
            });

            index++;
        }

        return resultado;
    }

    public async Task CriarEscalaAsync(int mes, int ano)
    {
        var dirigentes = await _context.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        if (!dirigentes.Any())
            throw new Exception("Nenhum dirigente ativo cadastrado.");

        var config = await _context.Configuracoes.FirstAsync();

        var escalas = GerarFila(
            mes,
            ano,
            dirigentes,
            config.UltimoDirigenteId
        );

        foreach (var escala in escalas)
        {
            bool jaExiste = await _context.Escalas
                .AnyAsync(e => e.Data == escala.Data);

            if (!jaExiste)
                _context.Escalas.Add(escala);

            config.UltimoDirigenteId = escala.DirigenteId;
        }

        await _context.SaveChangesAsync();
    }
}
