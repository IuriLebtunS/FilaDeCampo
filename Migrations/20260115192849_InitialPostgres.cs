using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FilaDeCampo.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Congregacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "text", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Congregacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UltimoDirigenteId = table.Column<int>(type: "integer", nullable: false),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configuracoes_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dirigentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    OrdemRodizio = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dirigentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dirigentes_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Escalas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DirigenteId = table.Column<int>(type: "integer", nullable: false),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: false),
                    Observacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Escalas_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Escalas_Dirigentes_DirigenteId",
                        column: x => x.DirigenteId,
                        principalTable: "Dirigentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configuracoes_CongregacaoId",
                table: "Configuracoes",
                column: "CongregacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Dirigentes_CongregacaoId",
                table: "Dirigentes",
                column: "CongregacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_CongregacaoId",
                table: "Escalas",
                column: "CongregacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_DirigenteId",
                table: "Escalas",
                column: "DirigenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "Escalas");

            migrationBuilder.DropTable(
                name: "Dirigentes");

            migrationBuilder.DropTable(
                name: "Congregacoes");
        }
    }
}
