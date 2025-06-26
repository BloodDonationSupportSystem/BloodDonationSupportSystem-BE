using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedTime",
                table: "Locations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedTime",
                table: "Locations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedTime",
                table: "Locations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationCapacities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCapacities", x => x.Id);
                    table.CheckConstraint("CK_LocationCapacity_TotalCapacity", "TotalCapacity >= 0");
                    table.ForeignKey(
                        name: "FK_LocationCapacities_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationOperatingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    MorningStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    MorningEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    AfternoonStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    AfternoonEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EveningStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EveningEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOperatingHours_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationStaffAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanManageCapacity = table.Column<bool>(type: "bit", nullable: false),
                    CanApproveAppointments = table.Column<bool>(type: "bit", nullable: false),
                    CanViewReports = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UnassignedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationStaffAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationStaffAssignments_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationStaffAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCapacities_EffectiveDate_ExpiryDate_IsActive",
                table: "LocationCapacities",
                columns: new[] { "EffectiveDate", "ExpiryDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCapacities_LocationId_TimeSlot_DayOfWeek",
                table: "LocationCapacities",
                columns: new[] { "LocationId", "TimeSlot", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationOperatingHours_LocationId_DayOfWeek",
                table: "LocationOperatingHours",
                columns: new[] { "LocationId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationStaffAssignments_LocationId_UserId",
                table: "LocationStaffAssignments",
                columns: new[] { "LocationId", "UserId" },
                unique: true,
                filter: "IsActive = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LocationStaffAssignments_LocationId_UserId_IsActive",
                table: "LocationStaffAssignments",
                columns: new[] { "LocationId", "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationStaffAssignments_UserId_IsActive",
                table: "LocationStaffAssignments",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationCapacities");

            migrationBuilder.DropTable(
                name: "LocationOperatingHours");

            migrationBuilder.DropTable(
                name: "LocationStaffAssignments");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "DeletedTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "LastUpdatedTime",
                table: "Locations");
        }
    }
}
