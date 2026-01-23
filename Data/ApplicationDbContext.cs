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
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<OrdenEntrada> OrdenesEntrada { get; set; }
        public DbSet<DetalleOrdenEntrada> DetallesOrdenEntrada { get; set; }
        public DbSet<Rol> Roles { get; set; }

        public DbSet<Sucursal> Sucursales { get; set; }
public DbSet<Proveedor> Proveedores { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapping to existing database tables (lowercase names)
            modelBuilder.Entity<User>().ToTable("usuario");
            modelBuilder.Entity<Producto>().ToTable("producto");
            modelBuilder.Entity<Orden>().ToTable("ordenes"); // Mapping restored
            modelBuilder.Entity<Categoria>().ToTable("categoria");
            modelBuilder.Entity<SubCategoria>().ToTable("subcategoria");
            modelBuilder.Entity<Proveedor>().ToTable("proveedor");
            modelBuilder.Entity<Rol>().ToTable("rol");
            modelBuilder.Entity<OrdenEntrada>().ToTable("ordenentrada");
            modelBuilder.Entity<DetalleOrdenEntrada>().ToTable("detalleordenentrada");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd(); // Esto le dice a EF que lo genera la base de datos
        }

    }
}
