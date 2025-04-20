using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeQuiz.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerSystemRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "player" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
