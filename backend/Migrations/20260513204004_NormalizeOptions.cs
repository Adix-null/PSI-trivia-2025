using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    QuizQuestionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Options_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Options_QuizQuestionId",
                table: "Options",
                column: "QuizQuestionId");

            // Data Migration: Extract JSON array elements and insert into the new Options table
            migrationBuilder.Sql(@"
                INSERT INTO ""Options"" (""OptionText"", ""QuizQuestionId"")
                SELECT value, ""Id""
                FROM ""QuizQuestions"", json_array_elements_text(""Options""::json)
            ");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "QuizQuestions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "QuizQuestions",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Data Migration: Aggregate options back into a JSON array string
            migrationBuilder.Sql(@"
                UPDATE ""QuizQuestions"" q
                SET ""Options"" = COALESCE((
                    SELECT json_agg(o.""OptionText"")::text
                    FROM ""Options"" o
                    WHERE o.""QuizQuestionId"" = q.""Id""
                ), '[]')
            ");

            migrationBuilder.DropTable(
                name: "Options");
        }
    }
}
