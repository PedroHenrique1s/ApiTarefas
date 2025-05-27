namespace ApiTarefas.Contexto;

using Microsoft.AspNetCore;
using ApiTarefas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class BancoDados(DbContextOptions options) : DbContext(options)
{
    public DbSet<Usuario> TabelaUsuario { get; set; }
    public DbSet<Tarefas> TabelaTarefas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasMany(c => c.ListaTarefas).WithOne(p => p.Usuario).HasForeignKey(p => p.UsuarioId);
        base.OnModelCreating(modelBuilder);
    }
}