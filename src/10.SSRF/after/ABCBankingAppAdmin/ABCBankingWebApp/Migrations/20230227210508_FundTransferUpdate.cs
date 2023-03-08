using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCBankingWebAppAdmin.Migrations
{
    /// <inheritdoc />
    public partial class FundTransferUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<Guid>(
            //    name: "ID",
            //    table: "FundTransfer",
            //    type: "uniqueidentifier",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "ID",
            //    table: "FundTransfer",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(Guid),
            //    oldType: "uniqueidentifier")
            //    .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
