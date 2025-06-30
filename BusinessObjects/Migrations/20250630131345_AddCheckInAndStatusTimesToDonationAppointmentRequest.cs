using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInAndStatusTimesToDonationAppointmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledTime",
                table: "DonationAppointmentRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckInTime",
                table: "DonationAppointmentRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedTime",
                table: "DonationAppointmentRequests",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledTime",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropColumn(
                name: "CompletedTime",
                table: "DonationAppointmentRequests");
        }
    }
}
