using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicePathToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "PaymentAmount",
            //    table: "Students");

            //migrationBuilder.DropColumn(
            //    name: "RazorpayPaymentId",
            //    table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "InvoicePath",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoicePath",
                table: "Students");

            //migrationBuilder.AddColumn<decimal>(
            //    name: "PaymentAmount",
            //    table: "Students",
            //    type: "decimal(18,2)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "RazorpayPaymentId",
            //    table: "Students",
            //    type: "nvarchar(100)",
            //    maxLength: 100,
            //    nullable: false,
            //    defaultValue: "");
        }
    }
}
