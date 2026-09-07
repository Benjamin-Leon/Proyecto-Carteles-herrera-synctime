using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class LetrerosHerreradbContext : DbContext
{
    // DbContext representa la base completa y contiene el mapeo entre entidades C#
    // y tablas, columnas, claves y relaciones de LetrerosHerreradb.
    public LetrerosHerreradbContext()
    {
    }

    public LetrerosHerreradbContext(DbContextOptions<LetrerosHerreradbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CotizacionDetalle> CotizacionDetalles { get; set; }

    public virtual DbSet<Cotizacione> Cotizaciones { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<FechasComprometida> FechasComprometidas { get; set; }

    public virtual DbSet<HorariosAtencion> HorariosAtencions { get; set; }

    public virtual DbSet<Materiale> Materiales { get; set; }

    public virtual DbSet<MovimientosInventario> MovimientosInventarios { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<PedidoMaterial> PedidoMaterials { get; set; }

    public virtual DbSet<ProductoMaterial> ProductoMaterials { get; set; }

    public virtual DbSet<ProductosServicio> ProductosServicios { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Solicitude> Solicitudes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("clientes");

            entity.HasIndex(e => e.Telefono, "UQ_clientes_telefono").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.CanalPreferido)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("web", "DF_clientes_canal")
                .HasColumnName("canal_preferido");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_clientes_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<CotizacionDetalle>(entity =>
        {
            entity.HasKey(e => e.IdDetalle);

            entity.ToTable("cotizacion_detalle");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1m, "DF_detalle_cantidad")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([cantidad]*[precio_unitario])", true)
                .HasColumnType("decimal(21, 4)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_cotizacion");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_producto");
        });

        modelBuilder.Entity<Cotizacione>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion);

            entity.ToTable("cotizaciones");

            entity.HasIndex(e => e.IdCliente, "IX_cotizaciones_cliente");

            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pendiente", "DF_cotizaciones_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_cotizaciones_fecha")
                .HasColumnName("fecha_emision");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");
            entity.Property(e => e.ValidezDias)
                .HasDefaultValue(15, "DF_cotizaciones_validez")
                .HasColumnName("validez_dias");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cotizaciones_cliente");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_cotizaciones_empleado");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdSolicitud)
                .HasConstraintName("FK_cotizaciones_solicitud");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado);

            entity.ToTable("empleados");

            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true, "DF_empleados_activo")
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("operario", "DF_empleados_rol")
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<FechasComprometida>(entity =>
        {
            entity.HasKey(e => e.IdFecha);

            entity.ToTable("fechas_comprometidas");

            entity.Property(e => e.IdFecha).HasColumnName("id_fecha");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("reservado", "DF_fechas_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEntrega).HasColumnName("fecha_entrega");
            entity.Property(e => e.HoraEstimada).HasColumnName("hora_estimada");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.FechasComprometida)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fechas_pedido");
        });

        modelBuilder.Entity<HorariosAtencion>(entity =>
        {
            entity.HasKey(e => e.IdHorario);

            entity.ToTable("horarios_atencion");

            entity.Property(e => e.IdHorario).HasColumnName("id_horario");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("disponible", "DF_horarios_estado")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.Nota)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nota");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.HorariosAtencions)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_horarios_empleado");
        });

        modelBuilder.Entity<Materiale>(entity =>
        {
            entity.HasKey(e => e.IdMaterial);

            entity.ToTable("materiales");

            entity.HasIndex(e => e.IdProveedor, "IX_materiales_proveedor");

            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("categoria");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("costo_unitario");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.StockActual)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("stock_minimo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("unidad_medida");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Materiales)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK_materiales_proveedor");
        });

        modelBuilder.Entity<MovimientosInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("movimientos_inventario");

            entity.HasIndex(e => new { e.IdMaterial, e.Fecha }, "IX_movimientos_material_fecha");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_movimientos_fecha")
                .HasColumnName("fecha");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("motivo");
            entity.Property(e => e.ReferenciaPedido).HasColumnName("referencia_pedido");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_movimientos_empleado");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_movimientos_material");

            entity.HasOne(d => d.ReferenciaPedidoNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.ReferenciaPedido)
                .HasConstraintName("FK_movimientos_pedido");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion);

            entity.ToTable("notificaciones");

            entity.HasIndex(e => e.IdSolicitud, "IX_notificaciones_solicitud");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.Canal)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("gmail", "DF_notificaciones_canal")
                .HasColumnName("canal");
            entity.Property(e => e.Destinatario)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("destinatario");
            entity.Property(e => e.EstadoEnvio)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("enviado", "DF_notificaciones_estado")
                .HasColumnName("estado_envio");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_notificaciones_fecha")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_notificaciones_solicitud");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido);

            entity.ToTable("pedidos");

            entity.HasIndex(e => e.IdCliente, "IX_pedidos_cliente");

            entity.HasIndex(e => e.Estado, "IX_pedidos_estado");

            entity.HasIndex(e => e.IdSolicitud, "UQ_pedidos_solicitud").IsUnique();

            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("en_proceso", "DF_pedidos_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_pedidos_fecha")
                .HasColumnName("fecha_pedido");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.IdEmpleadoAsignado).HasColumnName("id_empleado_asignado");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("media", "DF_pedidos_prioridad")
                .HasColumnName("prioridad");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pedidos_cliente");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdCotizacion)
                .HasConstraintName("FK_pedidos_cotizacion");

            entity.HasOne(d => d.IdEmpleadoAsignadoNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdEmpleadoAsignado)
                .HasConstraintName("FK_pedidos_empleado");

            entity.HasOne(d => d.IdSolicitudNavigation).WithOne(p => p.Pedido)
                .HasForeignKey<Pedido>(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pedidos_solicitud");
        });

        modelBuilder.Entity<PedidoMaterial>(entity =>
        {
            entity.HasKey(e => new { e.IdPedido, e.IdMaterial });

            entity.ToTable("pedido_material");

            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.CantidadUtilizada)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_utilizada");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.PedidoMaterials)
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pedido_material_material");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.PedidoMaterials)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pedido_material_pedido");
        });

        modelBuilder.Entity<ProductoMaterial>(entity =>
        {
            entity.HasKey(e => new { e.IdProducto, e.IdMaterial });

            entity.ToTable("producto_material");

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.CantidadRequerida)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_requerida");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.ProductoMaterials)
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_producto_material_material");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ProductoMaterials)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_producto_material_producto");
        });

        modelBuilder.Entity<ProductosServicio>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("productos_servicios");

            entity.HasIndex(e => e.Nombre, "UQ_productos_nombre").IsUnique();

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioBase)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_base");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("unidad_medida");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor);

            entity.ToTable("proveedores");

            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Contacto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("contacto");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Solicitude>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud);

            entity.ToTable("solicitudes");

            entity.HasIndex(e => e.IdCliente, "IX_solicitudes_cliente");

            entity.HasIndex(e => e.Estado, "IX_solicitudes_estado");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1, "DF_solicitudes_cantidad")
                .HasColumnName("cantidad");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pendiente", "DF_solicitudes_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaDecision).HasColumnName("fecha_decision");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_solicitudes_fecha")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.MaterialSolicitado)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("material_solicitado");
            entity.Property(e => e.NotasWhatsapp)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("notas_whatsapp");
            entity.Property(e => e.Tamano)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tamano");
            entity.Property(e => e.TelefonoContacto)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono_contacto");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Solicitudes)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_solicitudes_cliente");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Solicitudes)
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_solicitudes_producto");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "UQ_usuarios_email").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true, "DF_usuarios_activo")
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("operario", "DF_usuarios_rol")
                .HasColumnName("rol");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_usuarios_empleado");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
