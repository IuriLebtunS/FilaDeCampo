using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FilaDeCampo.Migrations
{
    /// <inheritdoc />
    public partial class CreateAudioVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesAudioVideo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UltimoOperadorId = table.Column<int>(type: "integer", nullable: true),
                    UltimoAjudanteId = table.Column<int>(type: "integer", nullable: true),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesAudioVideo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesAudioVideo_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TecnicosAudioVideo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    FuncaoPermitida = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    OrdemRodizio = table.Column<int>(type: "integer", nullable: false),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TecnicosAudioVideo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TecnicosAudioVideo_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscalasAudioVideo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TecnicoId = table.Column<int>(type: "integer", nullable: false),
                    Funcao = table.Column<int>(type: "integer", nullable: false),
                    CongregacaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalasAudioVideo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalasAudioVideo_TecnicosAudioVideo_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "TecnicosAudioVideo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesAudioVideo_CongregacaoId",
                table: "ConfiguracoesAudioVideo",
                column: "CongregacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasAudioVideo_TecnicoId",
                table: "EscalasAudioVideo",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_TecnicosAudioVideo_CongregacaoId",
                table: "TecnicosAudioVideo",
                column: "CongregacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesAudioVideo");

            migrationBuilder.DropTable(
                name: "EscalasAudioVideo");

            migrationBuilder.DropTable(
                name: "TecnicosAudioVideo");
        }
    }
}
