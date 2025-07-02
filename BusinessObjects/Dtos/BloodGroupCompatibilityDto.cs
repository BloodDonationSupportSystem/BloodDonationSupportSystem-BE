using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO for blood group compatibility information
    /// </summary>
    public class BloodGroupCompatibilityDto
    {
        /// <summary>
        /// ID of the blood group
        /// </summary>
        public Guid BloodGroupId { get; set; }
        
        /// <summary>
        /// Name of the blood group (e.g., "A+", "O-")
        /// </summary>
        public string BloodGroupName { get; set; } = string.Empty;
        
        /// <summary>
        /// List of blood groups that this blood group can donate to
        /// </summary>
        public List<BloodGroupInfoDto> CanDonateTo { get; set; } = new List<BloodGroupInfoDto>();
        
        /// <summary>
        /// List of blood groups that this blood group can receive from
        /// </summary>
        public List<BloodGroupInfoDto> CanReceiveFrom { get; set; } = new List<BloodGroupInfoDto>();
        
        /// <summary>
        /// Compatibility information for different blood components
        /// </summary>
        public List<ComponentCompatibilityDto> ComponentCompatibility { get; set; } = new List<ComponentCompatibilityDto>();
    }
    
    /// <summary>
    /// Simple DTO for blood group information
    /// </summary>
    public class BloodGroupInfoDto
    {
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO for blood component compatibility information
    /// </summary>
    public class ComponentCompatibilityDto
    {
        /// <summary>
        /// ID of the component type
        /// </summary>
        public Guid ComponentTypeId { get; set; }
        
        /// <summary>
        /// Name of the component type (e.g., "Red Blood Cells", "Plasma")
        /// </summary>
        public string ComponentTypeName { get; set; } = string.Empty;
        
        /// <summary>
        /// List of blood groups that can donate this component to the specified blood group
        /// </summary>
        public List<BloodGroupInfoDto> CompatibleDonors { get; set; } = new List<BloodGroupInfoDto>();
    }
    
    /// <summary>
    /// DTO for blood compatibility search request
    /// </summary>
    public class BloodCompatibilitySearchDto
    {
        [Required(ErrorMessage = "Recipient blood group ID is required")]
        public Guid RecipientBloodGroupId { get; set; }
        
        public Guid? ComponentTypeId { get; set; }
        
        /// <summary>
        /// Whether to search for whole blood compatibility or component-specific compatibility
        /// </summary>
        public bool IsWholeBloodSearch { get; set; } = true;
    }
}