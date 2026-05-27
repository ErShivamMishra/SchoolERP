using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixGeneratedIdCardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_StudentId_TeacherId",
                table: "GeneratedIdCards");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_StudentId",
                table: "GeneratedIdCards",
                columns: new[] { "SchoolId", "TemplateId", "StudentId" },
                unique: true,
                filter: "[StudentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_TeacherId",
                table: "GeneratedIdCards",
                columns: new[] { "SchoolId", "TemplateId", "TeacherId" },
                unique: true,
                filter: "[TeacherId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_StudentId",
                table: "GeneratedIdCards");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_TeacherId",
                table: "GeneratedIdCards");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedIdCards_SchoolId_TemplateId_StudentId_TeacherId",
                table: "GeneratedIdCards",
                columns: new[] { "SchoolId", "TemplateId", "StudentId", "TeacherId" },
                unique: true,
                filter: "[StudentId] IS NOT NULL AND [TeacherId] IS NOT NULL");
        }
    }
}
