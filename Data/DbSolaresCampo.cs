using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Models;

namespace FilaDeCampo.Data;

public class DbSolaresCampo : DbContext
{
    public DbSolaresCampo(DbContextOptions<DbSolaresCampo> options)
        : base(options) { }

    public DbSet<Dirigente> Dirigentes => Set<Dirigente>();
    public DbSet<EscalaDeSabado> Escalas => Set<EscalaDeSabado>();
    public DbSet<Configuracao> Configuracoes => Set<Configuracao>();
}