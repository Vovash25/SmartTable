namespace SmartTable.Client.Models;

public class Student
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    public string Citizenship { get; set; } = string.Empty;
    public string? PkkNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class Course
{
    public Guid Id { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

// Конкретний запланований термін проведення курсу (ADR/PL з 15.08 по 20.08 і т.д.)
public class CourseDate
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }

    // Необов'язково — конкретний термін курсу, на який записаний студент
    public Guid? CourseDateId { get; set; }
    public CourseDate? CourseDate { get; set; }

    public DateTime? ArrivalDate { get; set; } 
    public decimal CoursePrice { get; set; }
    public decimal? MedicalExamPrice { get; set; }
    public decimal? PsychologicalExamPrice { get; set; }
    public decimal? TranslationPrice { get; set; }
    public decimal? PkzPrice { get; set; }
    public decimal? PowerOfAttorneyPrice { get; set; }
    public string? CompanyName { get; set; }
    public bool HasInvoice { get; set; }
    public string? PaymentMethod { get; set; }
    public bool NeedsHotel { get; set; }
    public string? HotelStayRange { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    public string AttendanceStatus { get; set; } = "waiting_arrival";
    public string DocumentsStatus { get; set; } = "not_checked";
    
    public bool DocPassport { get; set; }
    public bool DocDrivingLicense { get; set; }
    public bool DocPkk { get; set; }
    public bool DocMedical { get; set; }
    
    public string PaymentStatus { get; set; } = "unpaid";
    public decimal AmountPaid { get; set; } = 0m;
    public string? PaymentNotes { get; set; }
    
    public bool SvcMedical { get; set; }
    public bool SvcPsychological { get; set; }
    public bool SvcTranslation { get; set; }
    public bool SvcPowerOfAttorney { get; set; }
    
    public string HotelStatus { get; set; } = "not_required";
    public DateTime? HotelFrom { get; set; }
    public DateTime? HotelTo { get; set; }
}

public class FinancialTransaction
{
    public Guid Id { get; set; }
    public Guid? EnrollmentId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    public string? InvoiceNumber { get; set; }
    public string? Description { get; set; }
}

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.Now;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class CandidateRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public string PhoneE164 { get; set; } = string.Empty;
    public string? PreferredContactLanguage { get; set; }
    public string? RegistrationSource { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string PrimaryContactMethod { get; set; } = string.Empty;
    public string? SecondaryContactMethods { get; set; }
    public string? ContactNotes { get; set; }
    public string? CourseVariantCode { get; set; }
    public string? CourseType { get; set; }
    public string? CourseLanguage { get; set; }
    public DateTime? PreferredCourseDate { get; set; }
    public DateTime? PlannedArrivalDate { get; set; }
    public string? DateNotes { get; set; }
    public string PayerType { get; set; } = "unknown";
    public string? CompanyName { get; set; }
    public string NeedsHotel { get; set; } = "unknown";
    public DateTime? HotelFrom { get; set; }
    public DateTime? HotelTo { get; set; }
    public string? HotelNotes { get; set; }
    public string Status { get; set; } = "new";
    public string? RegistrationNotes { get; set; }
    public string? InitialInformation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
