using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexiumCode.Migrations
{
    /// <inheritdoc />
    public partial class AddForumCategoryAndParentReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentReplyId",
                table: "ForumReplies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumReplies_ParentReplyId",
                table: "ForumReplies",
                column: "ParentReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumReplies_ForumReplies_ParentReplyId",
                table: "ForumReplies",
                column: "ParentReplyId",
                principalTable: "ForumReplies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumReplies_ForumReplies_ParentReplyId",
                table: "ForumReplies");

            migrationBuilder.DropIndex(
                name: "IX_ForumReplies_ParentReplyId",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "ParentReplyId",
                table: "ForumReplies");
        }
    }
}
