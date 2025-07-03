using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class BloodRequest : BaseEntity
{
    // Thông tin yêu cầu cơ bản
    public int QuantityUnits { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset RequestDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? NeededByDate { get; set; }

    // Phân biệt yêu cầu thường và khẩn cấp
    public bool IsEmergency { get; set; }

    // Trường thông tin cho yêu cầu khẩn cấp
    public string PatientName { get; set; }
    public string ContactInfo { get; set; }
    public string UrgencyLevel { get; set; } // Critical, High, Medium, Low
    public string Address { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string HospitalName { get; set; }
    public string MedicalNotes { get; set; }

    // Thông tin người yêu cầu
    public Guid? RequestedBy { get; set; } // Nullable cho yêu cầu khẩn cấp từ bên ngoài
    [ForeignKey("RequestedBy")]
    public virtual User User { get; set; }

    // Thông tin máu yêu cầu
    public Guid BloodGroupId { get; set; }
    [ForeignKey("BloodGroupId")]
    public virtual BloodGroup BloodGroup { get; set; }

    public Guid ComponentTypeId { get; set; }
    [ForeignKey("ComponentTypeId")]
    public virtual ComponentType ComponentType { get; set; }

    // Thông tin vị trí
    public Guid LocationId { get; set; }
    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; }

    // Các trường cơ bản
    public bool IsActive { get; set; } = true;
}