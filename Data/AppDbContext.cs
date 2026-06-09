using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Usuario>(options)
    {
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Painel> Paineis { get; set; } = null!;
        public DbSet<Sugestao> Sugestoes { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Painel>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Paineis)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefreshToken>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();
        }
    }
}
