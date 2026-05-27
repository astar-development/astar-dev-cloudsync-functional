using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStar.Dev.CloudSyncFunctional.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DriveId = table.Column<string>(type: "TEXT", nullable: false),
                    LastSyncedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    LocalSyncPath = table.Column<string>(type: "TEXT", nullable: false),
                    WorkerCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileClassificationRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Classification = table.Column<string>(type: "TEXT", nullable: false),
                    Keywords = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileClassificationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriveStates",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    DeltaLink = table.Column<string>(type: "TEXT", nullable: false),
                    LastCheckedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveStates", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_DriveStates_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncConflicts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteItemId = table.Column<string>(type: "TEXT", nullable: false),
                    LocalModifiedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    RemoteModifiedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncedItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    RemotePath = table.Column<string>(type: "TEXT", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteModifiedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    ETag = table.Column<string>(type: "TEXT", nullable: true),
                    IsFolder = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncedItems_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    RemotePath = table.Column<string>(type: "TEXT", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", nullable: false),
                    JobType = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobs_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    RemotePath = table.Column<string>(type: "TEXT", nullable: false),
                    RuleType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncRules_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncedItemClassifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SyncedItemId = table.Column<string>(type: "TEXT", nullable: false),
                    Classification = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncedItemClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncedItemClassifications_SyncedItems_SyncedItemId",
                        column: x => x.SyncedItemId,
                        principalTable: "SyncedItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_AccountId",
                table: "SyncConflicts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncedItemClassifications_SyncedItemId",
                table: "SyncedItemClassifications",
                column: "SyncedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncedItems_AccountId",
                table: "SyncedItems",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_AccountId",
                table: "SyncJobs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncRules_AccountId",
                table: "SyncRules",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriveStates");

            migrationBuilder.DropTable(
                name: "FileClassificationRules");

            migrationBuilder.DropTable(
                name: "SyncConflicts");

            migrationBuilder.DropTable(
                name: "SyncedItemClassifications");

            migrationBuilder.DropTable(
                name: "SyncJobs");

            migrationBuilder.DropTable(
                name: "SyncRules");

            migrationBuilder.DropTable(
                name: "SyncedItems");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
