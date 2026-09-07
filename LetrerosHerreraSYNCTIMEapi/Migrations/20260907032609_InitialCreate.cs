using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetrerosHerreraSYNCTIMEapi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    direccion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    canal_preferido = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "web")
                        .Annotation("Relational:DefaultConstraintName", "DF_clientes_canal"),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_clientes_fecha")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "empleados",
                columns: table => new
                {
                    id_empleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    rol = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "operario")
                        .Annotation("Relational:DefaultConstraintName", "DF_empleados_rol"),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_empleados_activo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados", x => x.id_empleado);
                });

            migrationBuilder.CreateTable(
                name: "productos_servicios",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    unidad_medida = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    precio_base = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos_servicios", x => x.id_producto);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    contacto = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    direccion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id_proveedor);
                });

            migrationBuilder.CreateTable(
                name: "horarios_atencion",
                columns: table => new
                {
                    id_horario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empleado = table.Column<int>(type: "int", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time", nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "disponible")
                        .Annotation("Relational:DefaultConstraintName", "DF_horarios_estado"),
                    nota = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios_atencion", x => x.id_horario);
                    table.ForeignKey(
                        name: "FK_horarios_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empleado = table.Column<int>(type: "int", nullable: true),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "operario")
                        .Annotation("Relational:DefaultConstraintName", "DF_usuarios_rol"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_usuarios_activo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                });

            migrationBuilder.CreateTable(
                name: "solicitudes",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_producto = table.Column<int>(type: "int", nullable: true),
                    tamano = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    material_solicitado = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    cantidad = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                        .Annotation("Relational:DefaultConstraintName", "DF_solicitudes_cantidad"),
                    telefono_contacto = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_solicitudes_fecha"),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "pendiente")
                        .Annotation("Relational:DefaultConstraintName", "DF_solicitudes_estado"),
                    fecha_decision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    notas_whatsapp = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK_solicitudes_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente");
                    table.ForeignKey(
                        name: "FK_solicitudes_producto",
                        column: x => x.id_producto,
                        principalTable: "productos_servicios",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "materiales",
                columns: table => new
                {
                    id_material = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_proveedor = table.Column<int>(type: "int", nullable: true),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    categoria = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    unidad_medida = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    stock_actual = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    stock_minimo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    costo_unitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materiales", x => x.id_material);
                    table.ForeignKey(
                        name: "FK_materiales_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "proveedores",
                        principalColumn: "id_proveedor");
                });

            migrationBuilder.CreateTable(
                name: "cotizaciones",
                columns: table => new
                {
                    id_cotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_solicitud = table.Column<int>(type: "int", nullable: true),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_empleado = table.Column<int>(type: "int", nullable: true),
                    fecha_emision = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_cotizaciones_fecha"),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "pendiente")
                        .Annotation("Relational:DefaultConstraintName", "DF_cotizaciones_estado"),
                    validez_dias = table.Column<int>(type: "int", nullable: false, defaultValue: 15)
                        .Annotation("Relational:DefaultConstraintName", "DF_cotizaciones_validez"),
                    total = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotizaciones", x => x.id_cotizacion);
                    table.ForeignKey(
                        name: "FK_cotizaciones_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente");
                    table.ForeignKey(
                        name: "FK_cotizaciones_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "FK_cotizaciones_solicitud",
                        column: x => x.id_solicitud,
                        principalTable: "solicitudes",
                        principalColumn: "id_solicitud");
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    id_notificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_solicitud = table.Column<int>(type: "int", nullable: false),
                    canal = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "gmail")
                        .Annotation("Relational:DefaultConstraintName", "DF_notificaciones_canal"),
                    destinatario = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_notificaciones_fecha"),
                    estado_envio = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "enviado")
                        .Annotation("Relational:DefaultConstraintName", "DF_notificaciones_estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK_notificaciones_solicitud",
                        column: x => x.id_solicitud,
                        principalTable: "solicitudes",
                        principalColumn: "id_solicitud");
                });

            migrationBuilder.CreateTable(
                name: "producto_material",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    id_material = table.Column<int>(type: "int", nullable: false),
                    cantidad_requerida = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_material", x => new { x.id_producto, x.id_material });
                    table.ForeignKey(
                        name: "FK_producto_material_material",
                        column: x => x.id_material,
                        principalTable: "materiales",
                        principalColumn: "id_material");
                    table.ForeignKey(
                        name: "FK_producto_material_producto",
                        column: x => x.id_producto,
                        principalTable: "productos_servicios",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "cotizacion_detalle",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cotizacion = table.Column<int>(type: "int", nullable: false),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    cantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 1m)
                        .Annotation("Relational:DefaultConstraintName", "DF_detalle_cantidad"),
                    precio_unitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(21,4)", nullable: true, computedColumnSql: "([cantidad]*[precio_unitario])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotizacion_detalle", x => x.id_detalle);
                    table.ForeignKey(
                        name: "FK_detalle_cotizacion",
                        column: x => x.id_cotizacion,
                        principalTable: "cotizaciones",
                        principalColumn: "id_cotizacion");
                    table.ForeignKey(
                        name: "FK_detalle_producto",
                        column: x => x.id_producto,
                        principalTable: "productos_servicios",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_solicitud = table.Column<int>(type: "int", nullable: false),
                    id_cotizacion = table.Column<int>(type: "int", nullable: true),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_empleado_asignado = table.Column<int>(type: "int", nullable: true),
                    fecha_pedido = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_pedidos_fecha"),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "en_proceso")
                        .Annotation("Relational:DefaultConstraintName", "DF_pedidos_estado"),
                    prioridad = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "media")
                        .Annotation("Relational:DefaultConstraintName", "DF_pedidos_prioridad")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id_pedido);
                    table.ForeignKey(
                        name: "FK_pedidos_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente");
                    table.ForeignKey(
                        name: "FK_pedidos_cotizacion",
                        column: x => x.id_cotizacion,
                        principalTable: "cotizaciones",
                        principalColumn: "id_cotizacion");
                    table.ForeignKey(
                        name: "FK_pedidos_empleado",
                        column: x => x.id_empleado_asignado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "FK_pedidos_solicitud",
                        column: x => x.id_solicitud,
                        principalTable: "solicitudes",
                        principalColumn: "id_solicitud");
                });

            migrationBuilder.CreateTable(
                name: "fechas_comprometidas",
                columns: table => new
                {
                    id_fecha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_pedido = table.Column<int>(type: "int", nullable: false),
                    fecha_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_estimada = table.Column<TimeOnly>(type: "time", nullable: true),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "reservado")
                        .Annotation("Relational:DefaultConstraintName", "DF_fechas_estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechas_comprometidas", x => x.id_fecha);
                    table.ForeignKey(
                        name: "FK_fechas_pedido",
                        column: x => x.id_pedido,
                        principalTable: "pedidos",
                        principalColumn: "id_pedido");
                });

            migrationBuilder.CreateTable(
                name: "movimientos_inventario",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_material = table.Column<int>(type: "int", nullable: false),
                    id_empleado = table.Column<int>(type: "int", nullable: true),
                    tipo_movimiento = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_movimientos_fecha"),
                    referencia_pedido = table.Column<int>(type: "int", nullable: true),
                    motivo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_inventario", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "FK_movimientos_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "FK_movimientos_material",
                        column: x => x.id_material,
                        principalTable: "materiales",
                        principalColumn: "id_material");
                    table.ForeignKey(
                        name: "FK_movimientos_pedido",
                        column: x => x.referencia_pedido,
                        principalTable: "pedidos",
                        principalColumn: "id_pedido");
                });

            migrationBuilder.CreateTable(
                name: "pedido_material",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "int", nullable: false),
                    id_material = table.Column<int>(type: "int", nullable: false),
                    cantidad_utilizada = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_material", x => new { x.id_pedido, x.id_material });
                    table.ForeignKey(
                        name: "FK_pedido_material_material",
                        column: x => x.id_material,
                        principalTable: "materiales",
                        principalColumn: "id_material");
                    table.ForeignKey(
                        name: "FK_pedido_material_pedido",
                        column: x => x.id_pedido,
                        principalTable: "pedidos",
                        principalColumn: "id_pedido");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_clientes_telefono",
                table: "clientes",
                column: "telefono",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cotizacion_detalle_id_cotizacion",
                table: "cotizacion_detalle",
                column: "id_cotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_cotizacion_detalle_id_producto",
                table: "cotizacion_detalle",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciones_cliente",
                table: "cotizaciones",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciones_id_empleado",
                table: "cotizaciones",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciones_id_solicitud",
                table: "cotizaciones",
                column: "id_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_fechas_comprometidas_id_pedido",
                table: "fechas_comprometidas",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_horarios_atencion_id_empleado",
                table: "horarios_atencion",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_materiales_proveedor",
                table: "materiales",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_id_empleado",
                table: "movimientos_inventario",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_referencia_pedido",
                table: "movimientos_inventario",
                column: "referencia_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_material_fecha",
                table: "movimientos_inventario",
                columns: new[] { "id_material", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_solicitud",
                table: "notificaciones",
                column: "id_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_material_id_material",
                table: "pedido_material",
                column: "id_material");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_cliente",
                table: "pedidos",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_estado",
                table: "pedidos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_id_cotizacion",
                table: "pedidos",
                column: "id_cotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_id_empleado_asignado",
                table: "pedidos",
                column: "id_empleado_asignado");

            migrationBuilder.CreateIndex(
                name: "UQ_pedidos_solicitud",
                table: "pedidos",
                column: "id_solicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_material_id_material",
                table: "producto_material",
                column: "id_material");

            migrationBuilder.CreateIndex(
                name: "UQ_productos_nombre",
                table: "productos_servicios",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_cliente",
                table: "solicitudes",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_estado",
                table: "solicitudes",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_id_producto",
                table: "solicitudes",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_empleado",
                table: "usuarios",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "UQ_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cotizacion_detalle");

            migrationBuilder.DropTable(
                name: "fechas_comprometidas");

            migrationBuilder.DropTable(
                name: "horarios_atencion");

            migrationBuilder.DropTable(
                name: "movimientos_inventario");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropTable(
                name: "pedido_material");

            migrationBuilder.DropTable(
                name: "producto_material");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "materiales");

            migrationBuilder.DropTable(
                name: "cotizaciones");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.DropTable(
                name: "empleados");

            migrationBuilder.DropTable(
                name: "solicitudes");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "productos_servicios");
        }
    }
}
