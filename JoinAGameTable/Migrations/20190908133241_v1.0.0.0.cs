using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JoinAGameTable.Migrations
{
    public partial class v1000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContentType = table.Column<string>(maxLength: 20, nullable: false),
                    Bucket = table.Column<string>(type: "TEXT", nullable: false),
                    Directory = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(maxLength: 125, nullable: false),
                    FirstName = table.Column<string>(maxLength: 35, nullable: false),
                    LastName = table.Column<string>(maxLength: 35, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 70, nullable: false),
                    Language = table.Column<string>(maxLength: 2, nullable: false),
                    AvatarId = table.Column<Guid>(nullable: true),
                    Password = table.Column<string>(maxLength: 95, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_file_metadata_AvatarId",
                        column: x => x.AvatarId,
                        principalTable: "file_metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    BannerId = table.Column<Guid>(nullable: true),
                    CoverId = table.Column<Guid>(nullable: true),
                    OwnerId = table.Column<Guid>(nullable: false),
                    BeginsAt = table.Column<DateTimeOffset>(nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(nullable: false),
                    PublicAt = table.Column<DateTimeOffset>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_file_metadata_BannerId",
                        column: x => x.BannerId,
                        principalTable: "file_metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_event_file_metadata_CoverId",
                        column: x => x.CoverId,
                        principalTable: "file_metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_event_account_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_table",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 35, nullable: false),
                    BeginAt = table.Column<DateTimeOffset>(nullable: false),
                    DurationEstimationLow = table.Column<int>(nullable: false),
                    DurationEstimationHigh = table.Column<int>(nullable: false),
                    NumberOfSeat = table.Column<int>(nullable: false),
                    GameType = table.Column<int>(nullable: false),
                    GameClassificationAge = table.Column<int>(nullable: false),
                    GameClassificationContent = table.Column<List<int>>(nullable: false),
                    EventId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_table_event_EventId",
                        column: x => x.EventId,
                        principalTable: "event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_table_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GameTableId = table.Column<Guid>(nullable: true),
                    Key = table.Column<int>(nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_table_metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_table_metadata_game_table_GameTableId",
                        column: x => x.GameTableId,
                        principalTable: "game_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_AvatarId",
                table: "account",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_account_Email",
                table: "account",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_BannerId",
                table: "event",
                column: "BannerId");

            migrationBuilder.CreateIndex(
                name: "IX_event_CoverId",
                table: "event",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_event_OwnerId",
                table: "event",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_event_Id_OwnerId",
                table: "event",
                columns: new[] { "Id", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_game_table_EventId",
                table: "game_table",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_game_table_Name",
                table: "game_table",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_table_metadata_GameTableId_Key",
                table: "game_table_metadata",
                columns: new[] { "GameTableId", "Key" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_table_metadata");

            migrationBuilder.DropTable(
                name: "game_table");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "file_metadata");
        }
    }
}
