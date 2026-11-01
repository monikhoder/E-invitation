using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    public partial class AddGuestInvitationToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationToken",
                table: "Guests",
                type: "TEXT",
                maxLength: 43,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guests_InvitationToken",
                table: "Guests",
                column: "InvitationToken",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Guests_InvitationToken",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "InvitationToken",
                table: "Guests");
        }
    }
}
