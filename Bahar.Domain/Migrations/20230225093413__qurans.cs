using Microsoft.EntityFrameworkCore.Migrations;

namespace Bahar.Domain.Migrations
{
    public partial class _qurans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Qurans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuranReaderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qurans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qurans_Readers_QuranReaderId",
                        column: x => x.QuranReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Readers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "قراءة الإمام نافع" },
                    { 2, "قراءة الإمام ابن كثير" },
                    { 3, "قراءة الإمام أبي عمرو" },
                    { 4, "قراءة الإمام ابن عامر" },
                    { 5, "قراءة الإمام عاصم" },
                    { 6, "قراءة الإمام حمزة" },
                    { 7, "قراءة الإمام الكسائي" },
                    { 8, "قراءة الإمام أبي جعفر" },
                    { 9, "قراءة يعقوب الحضرمي" },
                    { 10, "قراءة خلف البزار" }
                });

            migrationBuilder.InsertData(
                table: "Qurans",
                columns: new[] { "Id", "Name", "QuranReaderId" },
                values: new object[,]
                {
                    { 1, "مصحف قالون بقصر المنفصل وإسكان ميم الجمع", 1 },
                    { 23, "مصحف خلف بالسكت على الساكن المفصول", 6 },
                    { 24, "مصحف خلاد بن خالد بترك السكت مطلقا", 6 },
                    { 25, "مصحف خلاد بن خالد بالسكت المعروف له", 6 },
                    { 26, "مصحف قراءة الإمام الكسائي على المذهب الإجمالي", 7 },
                    { 27, "مصحف قراءة الإمام الكسائي على المذهب التفصيلي", 7 },
                    { 28, "مصحف الليث على المذهب الإجمالي في إمالة الهاء", 7 },
                    { 29, "مصحف الليث على المذهب التفصيلي في إمالة الهاء", 7 },
                    { 30, "مصحف الدوري على المذهب الإجمالي في إمالة الهاء", 7 },
                    { 31, "مصحف الدوري على المذهب التفصيلي في إمالة الهاء", 7 },
                    { 32, "مصحف قراءة الإمام يزيد بن القعقاع المدني", 8 },
                    { 33, "مصحف رواية أبي الحارث عيس بن وردان المدني", 8 },
                    { 34, "مصحف رواية أبي الربيع سليمان بن محمد بن جماز", 8 },
                    { 35, "مصحف قراءة الإمام يعقوب بن إسحاق الحضرمي", 9 },
                    { 36, "مصحف رواية محمد بن المتوكل المعروف برويس", 9 },
                    { 37, "مصحف رواية روح بن عبد المؤمن الهذلي البصري", 9 },
                    { 38, "مصحف قراءة الإمام خلف العاشر البزار البغدادي", 10 },
                    { 39, "مصحف إسحاق بن إبراهيم بن عثمان المروزي", 10 },
                    { 22, "مصحف خلف بترك السكت على الساكن المفصول", 6 },
                    { 40, "مصحف إدريس بن عبد الكريم الحداد البغدادي", 10 },
                    { 21, "مصحف قراءة الإمام حمزة بن حبيب الزيات الكوفي", 6 },
                    { 19, "مصحف حفص بن سليمان الأسدي - بحاشية يمنى", 5 },
                    { 2, "مصحف قالون بقصر المنفصل وصلة ميم الجمع", 1 },
                    { 3, "مصحف قالون بتوسط المنفصل وإسكان ميم الجمع", 1 },
                    { 4, "مصحف قالون بتوسط المنفصل وصلة ميم الجمع", 1 },
                    { 5, "مصحف ورش عن نافع المدني من طريق الأزرق", 1 },
                    { 6, "مصحف ورش عن نافع المدني من طريق الأصبهاني", 1 },
                    { 7, "مصحف قراءة الإمام عبدالله بن كثير المكي", 2 },
                    { 8, "مصحف رواية أحمد بن محمد المعروف بالبزي", 2 },
                    { 9, "مصحف رواية محمد بن عبدالرحمن المعروف بقنبل", 2 },
                    { 10, "مصحف قراءة الإمام أبي عمرو بن العلاء البصري", 3 },
                    { 11, "مصحف حفص بن عمر الدوري بتوسط المنفصل", 3 },
                    { 12, "مصحف حفص بن عمر الدوري بقصر المنفصل", 3 },
                    { 13, "مصحف أبي شعيب صالح بن زياد السوسي الرقي", 3 },
                    { 14, "مصحف قراءة الإمام عبدالله بن عامر الشامي", 4 },
                    { 15, "مصحف رواية هشام بن عمار السلمي", 4 },
                    { 16, "مصحف رواية عبدالله بن ذكوان القرشي", 4 },
                    { 17, "مصحف قراءة الإمام عاصم بن أبي النجود الكوفي", 5 },
                    { 18, "مصحف شعبة بن عياش الأسدي الكوفي", 5 },
                    { 20, "مصحف حفص بن سليمان الأسدي - بحاشية سفى", 5 },
                    { 41, "مصحف إدريس بالسكت العام من طريق المطوعي", 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Qurans_QuranReaderId",
                table: "Qurans",
                column: "QuranReaderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Qurans");

            migrationBuilder.DropTable(
                name: "Readers");
        }
    }
}
