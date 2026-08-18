using Consolidacao.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consolidacao.Infrastructure.Persistencia;

public sealed class ConsolidacaoDbContext : DbContext
{
    public ConsolidacaoDbContext(DbContextOptions<ConsolidacaoDbContext> options) : base(options)
    {
    }

    public DbSet<SaldoDiario> SaldosDiarios => Set<SaldoDiario>();
    public DbSet<LancamentoProcessado> LancamentosProcessados => Set<LancamentoProcessado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsolidacaoDbContext).Assembly);
    }
}

public sealed class SaldoDiarioConfiguration : IEntityTypeConfiguration<SaldoDiario>
{
    public void Configure(EntityTypeBuilder<SaldoDiario> builder)
    {
        builder.ToTable("saldos_diarios");
        builder.HasKey(x => x.Data);
        builder.Property(x => x.TotalCreditos).HasPrecision(18, 2);
        builder.Property(x => x.TotalDebitos).HasPrecision(18, 2);
        builder.Property(x => x.Saldo).HasPrecision(18, 2);
        builder.Property(x => x.QuantidadeLancamentos);
        builder.Property(x => x.AtualizadoEm);
    }
}

public sealed class LancamentoProcessadoConfiguration : IEntityTypeConfiguration<LancamentoProcessado>
{
    public void Configure(EntityTypeBuilder<LancamentoProcessado> builder)
    {
        builder.ToTable("lancamentos_processados");
        builder.HasKey(x => x.LancamentoId);
        builder.Property(x => x.Data).IsRequired();
        builder.Property(x => x.ProcessadoEm).IsRequired();
        builder.HasIndex(x => x.Data);
    }
}
