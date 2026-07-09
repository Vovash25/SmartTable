using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManagementApi.Data.Entities
{
    // --- 1. АУДИТ ЛОГИ ---
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("changed_by")]
        public string ChangedBy { get; set; } = string.Empty;

        [Column("entity_type")]
        public string EntityType { get; set; } = string.Empty;

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("entity_id")]
        public Guid EntityId { get; set; }
    }

    // --- 2. ФІНАНСОВІ ТРАНЗАКЦІЇ ---
    [Table("financial_transactions")]
    public class FinancialTransaction
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("enrollment_id")]
        public Guid? EnrollmentId { get; set; }
        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment? Enrollment { get; set; }

        [Required]
        [Column("transaction_type")]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("invoice_number")]
        public string? InvoiceNumber { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }
    
    // --- 3. СТУДЕНТИ ---
    [Table("students")]
    public class Student
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Column("citizenship")]
        public string Citizenship { get; set; } = string.Empty;

        [Column("pkk_number")]
        public string? PkkNumber { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }

    // --- 4. КУРСИ ---
    [Table("courses")]
    public class Course
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("course_type")]
        public string CourseType { get; set; } = string.Empty;

        [Required]
        [Column("language")]
        public string Language { get; set; } = string.Empty;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }

    // --- 5. РЕЄСТРАЦІЇ (Enrollments) ---
    // УВАГА: раніше цей клас не мав жодного [Column] атрибута, через що EF Core
    // генерував SQL із "сирими" PascalCase назвами (e."StudentId", e."Id" тощо),
    // а фізичні колонки в Postgres — snake_case (student_id, id...). Звідси помилки
    // "column e.StudentId does not exist". Додано явні [Table]/[Column] за тим самим
    // шаблоном, що вже використовується в Student/Course/CandidateRegistration.
    [Table("enrollments")]
    public class Enrollment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("student_id")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [Column("course_id")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        [Column("arrival_date")]
        public DateTime? ArrivalDate { get; set; }

        [Column("course_price")]
        public decimal CoursePrice { get; set; }

        [Column("medical_exam_price")]
        public decimal? MedicalExamPrice { get; set; }

        [Column("psychological_exam_price")]
        public decimal? PsychologicalExamPrice { get; set; }

        [Column("translation_price")]
        public decimal? TranslationPrice { get; set; }

        [Column("pkz_price")]
        public decimal? PkzPrice { get; set; }

        [Column("power_of_attorney_price")]
        public decimal? PowerOfAttorneyPrice { get; set; }

        [Column("company_name")]
        public string? CompanyName { get; set; }

        [Column("has_invoice")]
        public bool HasInvoice { get; set; }

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("needs_hotel")]
        public bool NeedsHotel { get; set; }

        [Column("hotel_stay_range")]
        public string? HotelStayRange { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Active"; // Status końcowy

        [Column("notes")]
        public string? Notes { get; set; } // Uwagi biura

        // === POLA KARTY OBSŁUGI (Etap 3) ===
        [Column("attendance_status")]
        public string AttendanceStatus { get; set; } = "waiting_arrival";

        [Column("documents_status")]
        public string DocumentsStatus { get; set; } = "not_checked";

        [Column("doc_passport")]
        public bool DocPassport { get; set; }

        [Column("doc_driving_license")]
        public bool DocDrivingLicense { get; set; }

        [Column("doc_pkk")]
        public bool DocPkk { get; set; }

        [Column("doc_medical")]
        public bool DocMedical { get; set; }

        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "unpaid";

        [Column("amount_paid")]
        public decimal AmountPaid { get; set; } = 0m;

        [Column("payment_notes")]
        public string? PaymentNotes { get; set; }

        [Column("svc_medical")]
        public bool SvcMedical { get; set; }

        [Column("svc_psychological")]
        public bool SvcPsychological { get; set; }

        [Column("svc_translation")]
        public bool SvcTranslation { get; set; }

        [Column("svc_power_of_attorney")]
        public bool SvcPowerOfAttorney { get; set; }

        [Column("hotel_status")]
        public string HotelStatus { get; set; } = "not_required";

        [Column("hotel_from")]
        public DateTime? HotelFrom { get; set; }

        [Column("hotel_to")]
        public DateTime? HotelTo { get; set; }
    }

    // --- 6. НОВА РЕЄСТРАЦІЯ КАНДИДАТІВ ЗГІДНО З ТЗ ---
    [Table("candidate_registrations")]
    public class CandidateRegistration
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [Column("national_number")]
        public string NationalNumber { get; set; } = string.Empty;

        [Required]
        [Column("phone_e164")]
        public string PhoneE164 { get; set; } = string.Empty;

        [Column("preferred_contact_language")]
        public string? PreferredContactLanguage { get; set; }

        [Column("registration_source")]
        public string? RegistrationSource { get; set; }

        [Column("assigned_user_id")]
        public Guid? AssignedUserId { get; set; }

        [Required]
        [Column("primary_contact_method")]
        public string PrimaryContactMethod { get; set; } = string.Empty;

        [Column("secondary_contact_methods")]
        public string? SecondaryContactMethods { get; set; }

        [Column("contact_notes")]
        public string? ContactNotes { get; set; }

        [Column("course_variant_code")]
        public string? CourseVariantCode { get; set; }

        [Column("course_type")]
        public string? CourseType { get; set; }

        [Column("course_language")]
        public string? CourseLanguage { get; set; }

        [Column("preferred_course_date")]
        public DateTime? PreferredCourseDate { get; set; }

        [Column("planned_arrival_date")]
        public DateTime? PlannedArrivalDate { get; set; }

        [Column("date_notes")]
        public string? DateNotes { get; set; }

        [Required]
        [Column("payer_type")]
        public string PayerType { get; set; } = "unknown";

        [Column("company_name")]
        public string? CompanyName { get; set; }

        [Required]
        [Column("needs_hotel")]
        public string NeedsHotel { get; set; } = "unknown";

        [Column("hotel_from")]
        public DateTime? HotelFrom { get; set; }

        [Column("hotel_to")]
        public DateTime? HotelTo { get; set; }

        [Column("hotel_notes")]
        public string? HotelNotes { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "new";

        [Column("registration_notes")]
        public string? RegistrationNotes { get; set; }

        [Column("initial_information")]
        public string? InitialInformation { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }
    }
}