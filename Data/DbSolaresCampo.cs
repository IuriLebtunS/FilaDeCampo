using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Models;

namespace FilaDeCampo.Data;

public class DbSolaresCampo : DbContext
{
    public DbSolaresCampo(DbContextOptions<DbSolaresCampo> options) : base(options) { }

    public DbSet<Dirigente> Dirigentes => Set<Dirigente>();
    public DbSet<EscalaDeSabado> Escalas => Set<EscalaDeSabado>();
    public DbSet<Configuracao> Configuracoes => Set<Configuracao>();
    public DbSet<Congregacao> Congregacoes => Set<Congregacao>();
    public DbSet<TecnicoAudioVideo> TecnicosAudioVideo => Set<TecnicoAudioVideo>();
    public DbSet<EscalaAudioVideo> EscalasAudioVideo => Set<EscalaAudioVideo>();
    public DbSet<ConfiguracaoAudioVideo> ConfiguracoesAudioVideo => Set<ConfiguracaoAudioVideo>();
}