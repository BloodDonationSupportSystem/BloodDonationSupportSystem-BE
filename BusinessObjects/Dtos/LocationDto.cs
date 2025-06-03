using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class LocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class CreateLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }

        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }

        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }
    }

    public class UpdateLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }

        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }

        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }
    }
}