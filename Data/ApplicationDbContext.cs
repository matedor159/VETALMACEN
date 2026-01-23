using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.Models;
namespace SisAlmacenProductos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<SubCategoria> SubCategorias { get; set; }

        public DbSet<Sucursal> Sucursales { get; set; }
public DbSet<Proveedor> Proveedores { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd(); // Esto le dice a EF que lo genera la base de datos

            // Un usuario solo puede tener 1 sucursal asociada
            modelBuilder.Entity<Sucursal>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // RUC único
            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.Ruc)
                .IsUnique();
        }

    }
}
