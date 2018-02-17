using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Conduit.Migrations
{
    public partial class AddUserIsFollowingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userIsFollowing",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false, maxLength: 450),
                    IsFollowingId = table.Column<string>(nullable: false, maxLength: 450)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userIsFollowing", x => new { x.UserId, x.IsFollowingId });
                    table.ForeignKey(
                        name: "FK_UserIsFollowing_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_UserIsFollowing_AspNetUsers_IsFollowingId",
                        column: x => x.IsFollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userIsFollowing");
        }
    }
}
