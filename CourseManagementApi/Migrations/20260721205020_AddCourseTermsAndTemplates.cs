using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTermsAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "course_term_id",
                table: "enrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "course_term_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "text", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: true),
                    frequency = table.Column<string>(type: "text", nullable: false),
                    generate_from = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    generate_to = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    default_duration_days = table.Column<int>(type: "integer", nullable: false),
                    default_price = table.Column<decimal>(type: "numeric", nullable: true),
                    seat_limit = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_term_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_term_templates_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_terms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    default_price = table.Column<decimal>(type: "numeric", nullable: true),
                    seat_limit = table.Column<int>(type: "integer", nullable: true),
                    trainer = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    manually_modified = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_terms", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_terms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_course_term_id",
                table: "enrollments",
                column: "course_term_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_term_templates_course_id",
                table: "course_term_templates",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_terms_course_id",
                table: "course_terms",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_enrollments_course_terms_course_term_id",
                table: "enrollments",
                column: "course_term_id",
                principalTable: "course_terms",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_enrollments_course_terms_course_term_id",
                table: "enrollments");

            migrationBuilder.DropTable(
                name: "course_term_templates");

            migrationBuilder.DropTable(
                name: "course_terms");

            migrationBuilder.DropIndex(
                name: "IX_enrollments_course_term_id",
                table: "enrollments");

            migrationBuilder.DropColumn(
                name: "course_term_id",
                table: "enrollments");
        }
    }
}
