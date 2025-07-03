using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO for sending emergency notifications to nearby donors
    /// </summary>
    public class EmergencyNotificationDto
    {
        [Required]
        public EmergencyBloodRequestDto EmergencyRequestDto { get; set; }
        
        [Required]
        public List<Guid> DonorIds { get; set; }
    }
}