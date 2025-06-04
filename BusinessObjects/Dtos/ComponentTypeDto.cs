using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class ComponentTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ShelfLifeDays { get; set; }
    }

    public class CreateComponentTypeDto
    {
        [Required(ErrorMessage = "Component type name is required")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Shelf life days is required")]
        [Range(1, 365, ErrorMessage = "Shelf life must be between 1 and 365 days")]
        public int ShelfLifeDays { get; set; }
    }

    public class UpdateComponentTypeDto
    {
        [Required(ErrorMessage = "Component type name is required")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Shelf life days is required")]
        [Range(1, 365, ErrorMessage = "Shelf life must be between 1 and 365 days")]
        public int ShelfLifeDays { get; set; }
    }
}