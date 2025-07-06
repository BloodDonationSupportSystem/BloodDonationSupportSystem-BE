using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedBloodRequestToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedBloodRequestId",
                table: "DonationAppointmentRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonationAppointmentRequests_RelatedBloodRequestId",
                table: "DonationAppointmentRequests",
                column: "RelatedBloodRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationAppointmentRequests_BloodRequests_RelatedBloodRequestId",
                table: "DonationAppointmentRequests",
                column: "RelatedBloodRequestId",
                principalTable: "BloodRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationAppointmentRequests_BloodRequests_RelatedBloodRequestId",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_DonationAppointmentRequests_RelatedBloodRequestId",
                table: "DonationAppointmentRequests");

            migrationBuilder.DropColumn(
                name: "RelatedBloodRequestId",
                table: "DonationAppointmentRequests");
        }
    }
}
