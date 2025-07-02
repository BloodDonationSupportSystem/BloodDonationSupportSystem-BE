using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonationEventDtos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionTaken",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BloodPressure",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckInTime",
                table: "DonationEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplicationDetails",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComplicationType",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DonationStartTime",
                table: "DonationEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "DonationEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HemoglobinLevel",
                table: "DonationEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsable",
                table: "DonationEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicalNotes",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "DonationEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "DonationEvents",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionTaken",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "BloodPressure",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "ComplicationDetails",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "ComplicationType",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "DonationStartTime",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "HemoglobinLevel",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "IsUsable",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "MedicalNotes",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "DonationEvents");
        }
    }
}
