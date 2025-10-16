using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Models;

public partial class TicketsDbContext : DbContext
{
    public TicketsDbContext()
    {
    }

    public TicketsDbContext(DbContextOptions<TicketsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchivosAdjunto> ArchivosAdjuntos { get; set; }

    public virtual DbSet<SeguimientoTicket> SeguimientoTickets { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("server=.\\SQLEXPRESS; database=TicketsDB; User Id=sa;Password=123456789; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivosAdjunto>(entity =>
        {
            entity.HasKey(e => e.IdArchivo).HasName("PK__Archivos__26B92111878A1BB0");

            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreArchivo).HasMaxLength(255);
            entity.Property(e => e.RutaArchivo).HasMaxLength(255);

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.ArchivosAdjuntos)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ArchivosA__IdTic__48CFD27E");
        });

        modelBuilder.Entity<SeguimientoTicket>(entity =>
        {
            entity.HasKey(e => e.IdSeguimiento).HasName("PK__Seguimie__5643F60FE151FF9A");

            entity.ToTable("SeguimientoTicket");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.SeguimientoTickets)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seguimien__IdTic__440B1D61");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.SeguimientoTickets)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seguimien__IdUsu__44FF419A");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PK__Tickets__4B93C7E7DAE939F6");

            entity.Property(e => e.Asunto).HasMaxLength(200);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Abierto");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Prioridad).HasMaxLength(20);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.TicketIdClienteNavigations)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__IdClien__3F466844");

            entity.HasOne(d => d.IdTecnicoNavigation).WithMany(p => p.TicketIdTecnicoNavigations)
                .HasForeignKey(d => d.IdTecnico)
                .HasConstraintName("FK__Tickets__IdTecni__403A8C7D");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF97A9B62BBF");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A196982CF33").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Clave).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Rol).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
