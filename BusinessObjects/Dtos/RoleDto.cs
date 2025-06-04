using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }

    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string RoleName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string RoleName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }
}