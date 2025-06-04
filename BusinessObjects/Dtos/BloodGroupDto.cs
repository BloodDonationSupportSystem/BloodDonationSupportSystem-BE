using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class BloodGroupDto
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
    }

    public class CreateBloodGroupDto
    {
        [Required(ErrorMessage = "Blood group name is required")]
        [StringLength(10, ErrorMessage = "Name cannot be longer than 10 characters")]
        public string GroupName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }

    public class UpdateBloodGroupDto
    {
        [Required(ErrorMessage = "Blood group name is required")]
        [StringLength(10, ErrorMessage = "Name cannot be longer than 10 characters")]
        public string GroupName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }
}