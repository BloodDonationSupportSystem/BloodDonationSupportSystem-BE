using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventoryIdFromDonationEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationEvents_BloodInventories_InventoryId",
                table: "DonationEvents");

            migrationBuilder.DropIndex(
                name: "IX_DonationEvents_InventoryId",
                table: "DonationEvents");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "DonationEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "FulfilledByStaffId",
                table: "BloodRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FulfilledDate",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPickedUp",
                table: "BloodRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PickupDate",
                table: "BloodRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupNotes",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FulfilledDate",
                table: "BloodInventories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FulfilledRequestId",
                table: "BloodInventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentNotes",
                table: "BloodInventories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventories_FulfilledRequestId",
                table: "BloodInventories",
                column: "FulfilledRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodInventories_BloodRequests_FulfilledRequestId",
                table: "BloodInventories",
                column: "FulfilledRequestId",
                principalTable: "BloodRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodInventories_BloodRequests_FulfilledRequestId",
                table: "BloodInventories");

            migrationBuilder.DropIndex(
                name: "IX_BloodInventories_FulfilledRequestId",
                table: "BloodInventories");

            migrationBuilder.DropColumn(
                name: "FulfilledByStaffId",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "FulfilledDate",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "IsPickedUp",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "PickupDate",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "PickupNotes",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "FulfilledDate",
                table: "BloodInventories");

            migrationBuilder.DropColumn(
                name: "FulfilledRequestId",
                table: "BloodInventories");

            migrationBuilder.DropColumn(
                name: "FulfillmentNotes",
                table: "BloodInventories");

            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "DonationEvents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonationEvents_InventoryId",
                table: "DonationEvents",
                column: "InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationEvents_BloodInventories_InventoryId",
                table: "DonationEvents",
                column: "InventoryId",
                principalTable: "BloodInventories",
                principalColumn: "Id");
        }
    }
}
