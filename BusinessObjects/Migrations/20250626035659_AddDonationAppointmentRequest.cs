using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationAppointmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonationAppointmentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferredDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PreferredTimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BloodGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComponentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InitiatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConfirmedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConfirmedTimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmedLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonorAccepted = table.Column<bool>(type: "bit", nullable: true),
                    DonorResponseAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DonorResponseNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationAppointmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_BloodDonationWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "BloodDonationWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_BloodGroups_BloodGroupId",
                        column: x => x.BloodGroupId,
                        principalTable: "BloodGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_DonorProfiles_DonorId",
                        column: x => x.DonorId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_Locations_ConfirmedLocationId",
                        column: x => x.ConfirmedLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_Users_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationAppointmentRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_BloodGroupId",
                table: "DonationAppointmentRequests",
                column: "BloodGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_ComponentTypeId",
                table: "DonationAppointmentRequests",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_ConfirmedLocationId",
                table: "DonationAppointmentRequests",
                column: "ConfirmedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_DonorId",
                table: "DonationAppointmentRequests",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_ExpiresAt",
                table: "DonationAppointmentRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_InitiatedByUserId",
                table: "DonationAppointmentRequests",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_IsUrgent_Priority",
                table: "DonationAppointmentRequests",
                columns: new[] { "IsUrgent", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_LocationId_PreferredDate",
                table: "DonationAppointmentRequests",
                columns: new[] { "LocationId", "PreferredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_ReviewedByUserId",
                table: "DonationAppointmentRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_Status_RequestType",
                table: "DonationAppointmentRequests",
                columns: new[] { "Status", "RequestType" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_WorkflowId",
                table: "DonationAppointmentRequests",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonationAppointmentRequests");
        }
    }
}
