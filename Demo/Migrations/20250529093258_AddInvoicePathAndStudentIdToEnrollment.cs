using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicePathAndStudentIdToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
    //        migrationBuilder.AddColumn<int>(
    //name: "StudentId",
    //table: "Enrollments",
    //nullable: false,
    //defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "InvoicePath",
            //    table: "Enrollments",
            //    nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
