using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinguaMeet.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(
                        type: "TEXT",
                        maxLength: 80,
                        nullable: false
                    ),
                    Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredLanguage = table.Column<string>(
                        type: "TEXT",
                        maxLength: 10,
                        nullable: false
                    ),
                    ProfileImage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomCode = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "MeetingParticipants",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedLanguage = table.Column<string>(
                        type: "TEXT",
                        maxLength: 10,
                        nullable: false
                    ),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingParticipants_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_MeetingParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "TranscriptSegments",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingId = table.Column<int>(type: "INTEGER", nullable: false),
                    SpeakerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalText = table.Column<string>(type: "TEXT", nullable: false),
                    SourceLanguage = table.Column<string>(
                        type: "TEXT",
                        maxLength: 10,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranscriptSegments_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_TranscriptSegments_Users_SpeakerId",
                        column: x => x.SpeakerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_MeetingId_UserId",
                table: "MeetingParticipants",
                columns: new[] { "MeetingId", "UserId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_UserId",
                table: "MeetingParticipants",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedBy",
                table: "Meetings",
                column: "CreatedBy"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_RoomCode",
                table: "Meetings",
                column: "RoomCode",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptSegments_MeetingId",
                table: "TranscriptSegments",
                column: "MeetingId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptSegments_SpeakerId",
                table: "TranscriptSegments",
                column: "SpeakerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MeetingParticipants");

            migrationBuilder.DropTable(name: "TranscriptSegments");

            migrationBuilder.DropTable(name: "Meetings");

            migrationBuilder.DropTable(name: "Users");
        }
    }
}
