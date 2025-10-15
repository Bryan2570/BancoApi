using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Models;

public partial class DbBancoPruebaContext : DbContext
{
    public DbBancoPruebaContext()
    {
    }

    public DbBancoPruebaContext(DbContextOptions<DbBancoPruebaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cuentum> Cuenta { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Cliente").HasKey(c => c.IdCliente);

            entity.Property(e => e.Contrasena)
                .HasMaxLength(100)
                .HasColumnName("contrasena");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdPersona).HasColumnName("idPersona");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany()
                .HasForeignKey(d => d.IdPersona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_Persona");
        });

        modelBuilder.Entity<Cuentum>(entity =>
        {
            entity.HasKey(e => e.IdCuenta);

            entity.Property(e => e.IdCuenta)
                .ValueGeneratedOnAdd()
                .HasColumnName("idCuenta");

            entity.Property(e => e.Estado).HasColumnName("estado");

            entity.Property(e => e.NumCuenta)
                .HasMaxLength(50)
                .HasColumnName("num_cuenta");

            entity.Property(e => e.SaldoInicial)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo_inicial");

            entity.Property(e => e.TipoCuenta)
                .HasMaxLength(50)
                .HasColumnName("tipo_cuenta");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("Movimiento");

            entity.Property(e => e.IdMovimiento).HasColumnName("idMovimiento");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Saldo)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(50)
                .HasColumnName("tipo_movimiento");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("valor");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.IdPersona);

            entity.ToTable("Persona");

            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.Direccion)
                .HasMaxLength(50)
                .HasColumnName("direccion");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.Genero)
                .HasMaxLength(50)
                .HasColumnName("genero");
            entity.Property(e => e.Identificacion)
                .HasMaxLength(50)
                .HasColumnName("identificacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .HasColumnName("telefono");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
