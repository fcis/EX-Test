using Core.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Assessment
{
    public class AssessmentDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public long FrameworkVersionId { get; set; }
        public string FrameworkName { get; set; } = string.Empty;
        public string FrameworkVersionName { get; set; } = string.Empty;
        public AssessmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public int ConformityItems { get; set; }
        public int NonConformityItems { get; set; }
        public int ConformityWithNotesItems { get; set; }
    }

    public class AssessmentListDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public long FrameworkVersionId { get; set; }
        public string FrameworkName { get; set; } = string.Empty;
        public string FrameworkVersionName { get; set; } = string.Empty;
        public AssessmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime CreationDate { get; set; }
        public int Progress { get; set; } // Percentage completion
    }

    public class CreateAssessmentDto
    {
        [Required]
        public long OrganizationId { get; set; }

        [Required]
        public long FrameworkId { get; set; }

        [Required]
        public long FrameworkVersionId { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateAssessmentDto
    {
        public AssessmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class AssessmentItemDto
    {
        public long Id { get; set; }
        public long AssessmentId { get; set; }
        public long ClauseId { get; set; }
        public string ClauseName { get; set; } = string.Empty;
        public ComplianceStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Notes { get; set; }
        public string? CorrectiveActions { get; set; }
        public long? AssignedDepartmentId { get; set; }
        public string? AssignedDepartmentName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
        public List<AssessmentItemDocumentDto> Documents { get; set; } = new List<AssessmentItemDocumentDto>();
        public List<AssessmentItemCheckListDto> CheckListItems { get; set; } = new List<AssessmentItemCheckListDto>();
    }

    public class UpdateAssessmentItemDto
    {
        [Required]
        public ComplianceStatus Status { get; set; }

        public string? Notes { get; set; }

        public string? CorrectiveActions { get; set; }

        public long? AssignedDepartmentId { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class AssessmentItemDocumentDto
    {
        public long Id { get; set; }
        public long AssessmentItemId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public long? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class UploadAssessmentDocumentDto
    {
        [Required]
        public long AssessmentItemId { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;

        public string? DocumentType { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public long? DepartmentId { get; set; }
    }

    public class AssessmentItemCheckListDto
    {
        public long Id { get; set; }
        public long AssessmentItemId { get; set; }
        public long CheckListId { get; set; }
        public string CheckListName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateAssessmentItemCheckListDto
    {
        [Required]
        public long CheckListId { get; set; }

        [Required]
        public bool IsChecked { get; set; }

        public string? Notes { get; set; }
    }

    // DTO for Gap Analysis
    public class GapAnalysisDto
    {
        public long AssessmentId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string FrameworkName { get; set; } = string.Empty;
        public string FrameworkVersionName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public int TotalControls { get; set; }
        public int ConformingControls { get; set; }
        public int NonConformingControls { get; set; }
        public int PartiallyConformingControls { get; set; }
        public int NotAssessedControls { get; set; }
        public decimal ConformityPercentage { get; set; }
        public List<DepartmentGapDto> DepartmentGaps { get; set; } = new List<DepartmentGapDto>();
        public List<CategoryGapDto> CategoryGaps { get; set; } = new List<CategoryGapDto>();
    }

    public class DepartmentGapDto
    {
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalControls { get; set; }
        public int ConformingControls { get; set; }
        public int NonConformingControls { get; set; }
        public int PartiallyConformingControls { get; set; }
        public int NotAssessedControls { get; set; }
        public decimal ConformityPercentage { get; set; }
    }

    public class CategoryGapDto
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalControls { get; set; }
        public int ConformingControls { get; set; }
        public int NonConformingControls { get; set; }
        public int PartiallyConformingControls { get; set; }
        public int NotAssessedControls { get; set; }
        public decimal ConformityPercentage { get; set; }
    }
}