using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    /// <summary>
    /// Contexto de almacenamiento en base de datos. Aca se definen los nombres de 
    /// las tablas, y los mapeos entre los objetos
    /// </summary>
    internal class StoreDbContext : DbContext
    {
        // Entidades del sistema educativo (legacy)
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<DummyEntity> DummyEntities { get; set; }

        // Entidades del sistema veterinario
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Raza> Razas { get; set; }
        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }

        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {
        }

        protected StoreDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Legacy entities
            modelBuilder.Entity<DummyEntity>().ToTable("DummyEntity");
            modelBuilder.Entity<Alumno>().ToTable("Alumnos");

            // Configuración de Especie
            modelBuilder.Entity<Especie>(entity =>
            {
                entity.ToTable("Especies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasMaxLength(200);
            });

            // Configuración de Raza
            modelBuilder.Entity<Raza>(entity =>
            {
                entity.ToTable("Razas");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedOnAdd();
                entity.Property(r => r.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Descripcion).HasMaxLength(200);

                entity.HasOne(r => r.Especie)
                    .WithMany(e => e.Razas)
                    .HasForeignKey(r => r.EspecieId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Propietario
            modelBuilder.Entity<Propietario>(entity =>
            {
                entity.ToTable("Propietarios");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(p => p.DNI).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Telefono).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Email).HasMaxLength(100);
                entity.Property(p => p.Direccion).HasMaxLength(200);

                entity.HasIndex(p => p.DNI).IsUnique();
            });

            // Configuración de Paciente
            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.ToTable("Pacientes");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Sexo).IsRequired().HasMaxLength(1);
                entity.Property(p => p.FotoUrl).HasMaxLength(500);
                entity.Property(p => p.Observaciones).HasMaxLength(1000);

                entity.HasOne(p => p.Especie)
                    .WithMany(e => e.Pacientes)
                    .HasForeignKey(p => p.EspecieId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Raza)
                    .WithMany(r => r.Pacientes)
                    .HasForeignKey(p => p.RazaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Propietario)
                    .WithMany(pr => pr.Mascotas)
                    .HasForeignKey(p => p.PropietarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

