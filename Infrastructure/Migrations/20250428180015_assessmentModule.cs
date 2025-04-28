using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class assessmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    FrameworkVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<long>(type: "bigint", nullable: false),
                    LastModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationUser = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assessment_framework_version_FrameworkVersionId",
                        column: x => x.FrameworkVersionId,
                        principalTable: "framework_version",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_item",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    ClauseId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectiveActions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignedDepartmentId = table.Column<long>(type: "bigint", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<long>(type: "bigint", nullable: false),
                    LastModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationUser = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assessment_item_assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assessment_item_clause_ClauseId",
                        column: x => x.ClauseId,
                        principalTable: "clause",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_item_organization_departments_AssignedDepartmentId",
                        column: x => x.AssignedDepartmentId,
                        principalTable: "organization_departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_item_checklist",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentItemId = table.Column<long>(type: "bigint", nullable: false),
                    CheckListId = table.Column<long>(type: "bigint", nullable: false),
                    IsChecked = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<long>(type: "bigint", nullable: false),
                    LastModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationUser = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_item_checklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assessment_item_checklist_assessment_item_AssessmentItemId",
                        column: x => x.AssessmentItemId,
                        principalTable: "assessment_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assessment_item_checklist_clause_check_list_CheckListId",
                        column: x => x.CheckListId,
                        principalTable: "clause_check_list",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_item_document",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentItemId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<long>(type: "bigint", nullable: false),
                    LastModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationUser = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_item_document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assessment_item_document_assessment_item_AssessmentItemId",
                        column: x => x.AssessmentItemId,
                        principalTable: "assessment_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assessment_item_document_organization_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "organization_departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_FrameworkVersionId",
                table: "assessment",
                column: "FrameworkVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_OrganizationId",
                table: "assessment",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_AssessmentId",
                table: "assessment_item",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_AssignedDepartmentId",
                table: "assessment_item",
                column: "AssignedDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_ClauseId",
                table: "assessment_item",
                column: "ClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_checklist_AssessmentItemId",
                table: "assessment_item_checklist",
                column: "AssessmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_checklist_CheckListId",
                table: "assessment_item_checklist",
                column: "CheckListId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_document_AssessmentItemId",
                table: "assessment_item_document",
                column: "AssessmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_item_document_DepartmentId",
                table: "assessment_item_document",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_item_checklist");

            migrationBuilder.DropTable(
                name: "assessment_item_document");

            migrationBuilder.DropTable(
                name: "assessment_item");

            migrationBuilder.DropTable(
                name: "assessment");
        }
    }
}
