using System;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    public class DonorProfileDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public DateTimeOffset LastDonationDate { get; set; }
        public string HealthStatus { get; set; }
        public DateTimeOffset LastHealthCheckDate { get; set; }
        public int TotalDonations { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateDonorProfileDto
    {
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public DateTimeOffset LastDonationDate { get; set; }
        public string HealthStatus { get; set; }
        public DateTimeOffset LastHealthCheckDate { get; set; }
        public int TotalDonations { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Guid UserId { get; set; }
        public Guid BloodGroupId { get; set; }
    }

    public class UpdateDonorProfileDto
    {
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public DateTimeOffset LastDonationDate { get; set; }
        public string HealthStatus { get; set; }
        public DateTimeOffset LastHealthCheckDate { get; set; }
        public int TotalDonations { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Guid BloodGroupId { get; set; }
    }
    
    public class DonorProfileParameters : PaginationParameters
    {
        public string BloodGroup { get; set; }
        public string HealthStatus { get; set; }
        public int? MinimumDonations { get; set; }
    }
}