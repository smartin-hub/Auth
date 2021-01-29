using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class AlphaShopDbContext : DbContext
    {
        public AlphaShopDbContext(DbContextOptions<AlphaShopDbContext> options)
            : base(options)
        {
            //Database.Migrate();
        }

        public virtual DbSet<Utenti> Utenti { get; set; }
        public virtual DbSet<Profili> Profili { get; set; }

        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Utenti>()
                .HasKey(a => new {a.CodFidelity});

            modelBuilder.Entity<Profili>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<Profili>()
                .HasOne<Utenti>(s => s.Utente)
                .WithMany(g => g.Profili)
                .HasForeignKey(g => g.CodFidelity);

            

        }
    }
}