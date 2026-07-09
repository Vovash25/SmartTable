using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    password_salt = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "candidate_registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    country_code = table.Column<string>(type: "text", nullable: false),
                    national_number = table.Column<string>(type: "text", nullable: false),
                    phone_e164 = table.Column<string>(type: "text", nullable: false),
                    preferred_contact_language = table.Column<string>(type: "text", nullable: true),
                    registration_source = table.Column<string>(type: "text", nullable: true),
                    assigned_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    primary_contact_method = table.Column<string>(type: "text", nullable: false),
                    secondary_contact_methods = table.Column<string>(type: "text", nullable: true),
                    contact_notes = table.Column<string>(type: "text", nullable: true),
                    course_variant_code = table.Column<string>(type: "text", nullable: true),
                    course_type = table.Column<string>(type: "text", nullable: true),
                    course_language = table.Column<string>(type: "text", nullable: true),
                    preferred_course_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    planned_arrival_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    date_notes = table.Column<string>(type: "text", nullable: true),
                    payer_type = table.Column<string>(type: "text", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    needs_hotel = table.Column<string>(type: "text", nullable: false),
                    hotel_from = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    hotel_to = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    hotel_notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    registration_notes = table.Column<string>(type: "text", nullable: true),
                    initial_information = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_registrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_type = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    citizenship = table.Column<string>(type: "text", nullable: false),
                    pkk_number = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arrival_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    course_price = table.Column<decimal>(type: "numeric", nullable: false),
                    medical_exam_price = table.Column<decimal>(type: "numeric", nullable: true),
                    psychological_exam_price = table.Column<decimal>(type: "numeric", nullable: true),
                    translation_price = table.Column<decimal>(type: "numeric", nullable: true),
                    pkz_price = table.Column<decimal>(type: "numeric", nullable: true),
                    power_of_attorney_price = table.Column<decimal>(type: "numeric", nullable: true),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    has_invoice = table.Column<bool>(type: "boolean", nullable: false),
                    payment_method = table.Column<string>(type: "text", nullable: true),
                    needs_hotel = table.Column<bool>(type: "boolean", nullable: false),
                    hotel_stay_range = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    attendance_status = table.Column<string>(type: "text", nullable: false),
                    documents_status = table.Column<string>(type: "text", nullable: false),
                    doc_passport = table.Column<bool>(type: "boolean", nullable: false),
                    doc_driving_license = table.Column<bool>(type: "boolean", nullable: false),
                    doc_pkk = table.Column<bool>(type: "boolean", nullable: false),
                    doc_medical = table.Column<bool>(type: "boolean", nullable: false),
                    payment_status = table.Column<string>(type: "text", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_notes = table.Column<string>(type: "text", nullable: true),
                    svc_medical = table.Column<bool>(type: "boolean", nullable: false),
                    svc_psychological = table.Column<bool>(type: "boolean", nullable: false),
                    svc_translation = table.Column<bool>(type: "boolean", nullable: false),
                    svc_power_of_attorney = table.Column<bool>(type: "boolean", nullable: false),
                    hotel_status = table.Column<string>(type: "text", nullable: false),
                    hotel_from = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    hotel_to = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollments_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enrollments_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "financial_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_type = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    invoice_number = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_transactions_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_users_username",
                table: "app_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidate_registrations_phone_e164",
                table: "candidate_registrations",
                column: "phone_e164");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_course_id",
                table: "enrollments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_student_id_course_id",
                table: "enrollments",
                columns: new[] { "student_id", "course_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_enrollment_id",
                table: "financial_transactions",
                column: "enrollment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_users");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "candidate_registrations");

            migrationBuilder.DropTable(
                name: "financial_transactions");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "students");
        }
    }
}
