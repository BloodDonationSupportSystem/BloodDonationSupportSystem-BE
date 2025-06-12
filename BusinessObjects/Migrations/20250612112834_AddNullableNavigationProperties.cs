using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestMatches");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EmergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "EmergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EmergencyRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "EmergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "EmergencyRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "EmergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MedicalNotes",
                table: "EmergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BloodDonationWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DonorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BloodGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppointmentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AppointmentLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppointmentConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    DonationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DonationLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuantityDonated = table.Column<double>(type: "float", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodDonationWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodDonationWorkflows_BloodGroups_BloodGroupId",
                        column: x => x.BloodGroupId,
                        principalTable: "BloodGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodDonationWorkflows_BloodInventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "BloodInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodDonationWorkflows_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodDonationWorkflows_DonorProfiles_DonorId",
                        column: x => x.DonorId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonorReminderSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnableReminders = table.Column<bool>(type: "bit", nullable: false),
                    DaysBeforeEligible = table.Column<int>(type: "int", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    InAppNotifications = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastReminderSentTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorReminderSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorReminderSettings_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyRequests_LocationId",
                table: "EmergencyRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_BloodGroupId_ComponentTypeId",
                table: "BloodDonationWorkflows",
                columns: new[] { "BloodGroupId", "ComponentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_ComponentTypeId",
                table: "BloodDonationWorkflows",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_DonorId",
                table: "BloodDonationWorkflows",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_InventoryId",
                table: "BloodDonationWorkflows",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_RequestId",
                table: "BloodDonationWorkflows",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationWorkflows_Status_IsActive",
                table: "BloodDonationWorkflows",
                columns: new[] { "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorReminderSettings_DonorProfileId",
                table: "DonorReminderSettings",
                column: "DonorProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorReminderSettings_EnableReminders_LastReminderSentTime",
                table: "DonorReminderSettings",
                columns: new[] { "EnableReminders", "LastReminderSentTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyRequests_Locations_LocationId",
                table: "EmergencyRequests",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyRequests_Locations_LocationId",
                table: "EmergencyRequests");

            migrationBuilder.DropTable(
                name: "BloodDonationWorkflows");

            migrationBuilder.DropTable(
                name: "DonorReminderSettings");

            migrationBuilder.DropIndex(
                name: "IX_EmergencyRequests_LocationId",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "EmergencyRequests");

            migrationBuilder.DropColumn(
                name: "MedicalNotes",
                table: "EmergencyRequests");

            migrationBuilder.CreateTable(
                name: "RequestMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonationEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmergencyRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MatchDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UnitsAssigned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestMatches_BloodRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestMatches_DonationEvents_DonationEventId",
                        column: x => x.DonationEventId,
                        principalTable: "DonationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestMatches_EmergencyRequests_EmergencyRequestId",
                        column: x => x.EmergencyRequestId,
                        principalTable: "EmergencyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestMatches_DonationEventId",
                table: "RequestMatches",
                column: "DonationEventId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMatches_EmergencyRequestId",
                table: "RequestMatches",
                column: "EmergencyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMatches_RequestId",
                table: "RequestMatches",
                column: "RequestId");
        }
    }
}
