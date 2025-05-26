using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetcoreapp.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecretKey",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTwoFactorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecretKey",
                table: "AspNetUsers");
        }
    }
}
