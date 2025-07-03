using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class MergeEmergencyRequestToBloodRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyRequests");

            migrationBuilder.DropIndex(
                name: "IX_BloodRequests_BloodGroupId_ComponentTypeId_Status",
                table: "BloodRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestedBy",
                table: "BloodRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NeededByDate",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedTime",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedTime",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BloodRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergency",
                table: "BloodRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedTime",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MedicalNotes",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UrgencyLevel",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_BloodGroupId_ComponentTypeId_Status_IsEmergency",
                table: "BloodRequests",
                columns: new[] { "BloodGroupId", "ComponentTypeId", "Status", "IsEmergency" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BloodRequests_BloodGroupId_ComponentTypeId_Status_IsEmergency",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "DeletedTime",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "IsEmergency",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "LastUpdatedTime",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "MedicalNotes",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "UrgencyLevel",
                table: "BloodRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestedBy",
                table: "BloodRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NeededByDate",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EmergencyRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BloodGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HospitalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityUnits = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyRequests_BloodGroups_BloodGroupId",
                        column: x => x.BloodGroupId,
                        principalTable: "BloodGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyRequests_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_BloodGroupId_ComponentTypeId_Status",
                table: "BloodRequests",
                columns: new[] { "BloodGroupId", "ComponentTypeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyRequests_BloodGroupId",
                table: "EmergencyRequests",
                column: "BloodGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyRequests_ComponentTypeId",
                table: "EmergencyRequests",
                column: "ComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyRequests_LocationId",
                table: "EmergencyRequests",
                column: "LocationId");
        }
    }
}
