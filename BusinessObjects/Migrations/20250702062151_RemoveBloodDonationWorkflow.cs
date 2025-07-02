using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBloodDonationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationAppointmentRequests_BloodDonationWorkflows_WorkflowId",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropTable(
                name: "BloodDonationWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_DonationAppointmentRequests_WorkflowId",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "DonationAppointmentRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DonationEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "DonorId",
                table: "DonationEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "CollectedAt",
                table: "DonationEvents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "AppointmentConfirmed",
                table: "DonationEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppointmentDate",
                table: "DonationEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentLocation",
                table: "DonationEvents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedTime",
                table: "DonationEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DonationDate",
                table: "DonationEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationLocation",
                table: "DonationEvents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "DonationEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DonationEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "DonationEvents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "QuantityDonated",
                table: "DonationEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "DonationEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "DonationEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "DonationEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusDescription",
                table: "DonationEvents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DonationEvents_InventoryId",
                table: "DonationEvents",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationEvents_StaffId",
                table: "DonationEvents",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationEvents_BloodInventories_InventoryId",
                table: "DonationEvents",
                column: "InventoryId",
                principalTable: "BloodInventories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationEvents_Users_StaffId",
                table: "DonationEvents",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationEvents_BloodInventories_InventoryId",
                table: "DonationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationEvents_Users_StaffId",
                table: "DonationEvents");

            migrationBuilder.DropIndex(
                name: "IX_DonationEvents_InventoryId",
                table: "DonationEvents");

            migrationBuilder.DropIndex(
                name: "IX_DonationEvents_StaffId",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "AppointmentConfirmed",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "AppointmentLocation",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "CompletedTime",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "DonationDate",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "DonationLocation",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "QuantityDonated",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "StatusDescription",
                table: "DonationEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "DonorId",
                table: "DonationEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CollectedAt",
                table: "DonationEvents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowId",
                table: "DonationAppointmentRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BloodDonationWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BloodGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryId = table.Column<int>(type: "int", nullable: true),
                    AppointmentConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    AppointmentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AppointmentLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CompletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DonationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DonationLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuantityDonated = table.Column<double>(type: "float", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_WorkflowId",
                table: "DonationAppointmentRequests",
                column: "WorkflowId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_DonationAppointmentRequests_BloodDonationWorkflows_WorkflowId",
                table: "DonationAppointmentRequests",
                column: "WorkflowId",
                principalTable: "BloodDonationWorkflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
