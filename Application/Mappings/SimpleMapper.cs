using Application.DTOs.Assessment;
using Application.DTOs.Framework;
using Application.DTOs.Category;
using Application.DTOs.Clause;
using Application.DTOs.Organization;
using Application.DTOs.User;
using Application.DTOs.Audit;
using Core.Entities;
using Core.Entities.Identitiy;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Application.Mappings
{
    public static class SimpleMapper
    {
        // Framework mappings
        public static FrameworkDto ToDto(this Framework entity)
        {
            if (entity == null) return null;

            return new FrameworkDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.Status,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                Versions = entity.Versions?.Select(v => v.ToDto()).ToList() ?? new List<FrameworkVersionDto>()
            };
        }

        public static FrameworkListDto ToListDto(this Framework entity)
        {
            if (entity == null) return null;

            return new FrameworkListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.Status,
                CreationDate = entity.CreationDate,
                VersionCount = entity.Versions?.Count ?? 0
            };
        }

        public static Framework ToEntity(this CreateFrameworkDto dto)
        {
            if (dto == null) return null;

            return new Framework
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        // FrameworkVersion mappings
        public static FrameworkVersionDto ToDto(this FrameworkVersion entity)
        {
            if (entity == null) return null;

            return new FrameworkVersionDto
            {
                Id = entity.Id,
                FrameworkId = entity.FrameworkId,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.Status,
                VersionDate = entity.VersionDate,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser
            };
        }

        public static FrameworkVersion ToEntity(this CreateFrameworkVersionDto dto)
        {
            if (dto == null) return null;

            return new FrameworkVersion
            {
                FrameworkId = dto.FrameworkId,
                Name = dto.Name,
                Description = dto.Description,
                VersionDate = dto.VersionDate
            };
        }

        // Category mappings
        public static CategoryDto ToDto(this Category entity)
        {
            if (entity == null) return null;

            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                FrameworksCount = entity.Frameworks?.Count ?? 0
            };
        }

        public static Category ToEntity(this CreateCategoryDto dto)
        {
            if (dto == null) return null;

            return new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        // Clause mappings
        public static ClauseDto ToDto(this Clause entity)
        {
            if (entity == null) return null;

            return new ClauseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                CheckLists = entity.CheckLists?.Select(c => c.ToDto()).ToList() ?? new List<ClauseCheckListDto>()
            };
        }

        public static Clause ToEntity(this CreateClauseDto dto)
        {
            if (dto == null) return null;

            return new Clause
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        // ClauseCheckList mappings
        public static ClauseCheckListDto ToDto(this ClauseCheckList entity)
        {
            if (entity == null) return null;

            return new ClauseCheckListDto
            {
                Id = entity.Id,
                ClauseId = entity.ClauseId,
                Name = entity.Name,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser
            };
        }

        public static ClauseCheckList ToEntity(this CreateClauseCheckListDto dto)
        {
            if (dto == null) return null;

            return new ClauseCheckList
            {
                ClauseId = dto.ClauseId,
                Name = dto.Name
            };
        }

        // Organization mappings
        public static OrganizationDto ToDto(this Organization entity)
        {
            if (entity == null) return null;

            return new OrganizationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Website = entity.Website,
                Email = entity.Email,
                Phone = entity.Phone,
                EmployeesCount = entity.EmployeesCount,
                Industry = entity.Industry,
                Description = entity.Description,
                Status = entity.Status,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                Departments = entity.Departments?.Select(d => d.ToDto()).ToList() ?? new List<OrganizationDepartmentDto>()
            };
        }

        public static OrganizationListDto ToListDto(this Organization entity)
        {
            if (entity == null) return null;

            return new OrganizationListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Industry = entity.Industry,
                Status = entity.Status,
                CreationDate = entity.CreationDate,
                DepartmentsCount = entity.Departments?.Count(d => !d.Deleted) ?? 0,
                UsersCount = entity.Users?.Count ?? 0,
                FrameworksCount = entity.Memberships?.Count(m => m.Status != Core.Enums.OrganizationMembershipStatus.DELETED) ?? 0,
                LastModificationDate = entity.LastModificationDate
            };
        }

        public static Organization ToEntity(this CreateOrganizationDto dto)
        {
            if (dto == null) return null;

            return new Organization
            {
                Name = dto.Name,
                Website = dto.Website,
                Email = dto.Email,
                Phone = dto.Phone,
                EmployeesCount = dto.EmployeesCount,
                Industry = dto.Industry,
                Description = dto.Description
            };
        }

        // OrganizationDepartment mappings
        public static OrganizationDepartmentDto ToDto(this OrganizationDepartments entity)
        {
            if (entity == null) return null;

            return new OrganizationDepartmentDto
            {
                Id = entity.Id,
                OrganizationId = entity.OrganizationId,
                Name = entity.Name,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser
            };
        }

        // User mappings
        public static UserDto ToDto(this User entity)
        {
            if (entity == null) return null;

            return new UserDto
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                RoleId = entity.RoleId,
                RoleName = entity.Role?.Name ?? string.Empty,
                OrganizationId = entity.OrganizationId,
                OrganizationDepartmentId = entity.OrganizationDepartmentId,
                Name = entity.Name,
                LastLogin = entity.LastLogin,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser
            };
        }

        public static UserListDto ToListDto(this User entity)
        {
            if (entity == null) return null;

            return new UserListDto
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                Name = entity.Name,
                RoleName = entity.Role?.Name ?? string.Empty,
                OrganizationName = entity.Organization?.Name,
                Status = entity.Status,
                LastLogin = entity.LastLogin,
                CreatedAt = entity.CreatedAt
            };
        }

        // Role mappings
        public static RoleDto ToDto(this Role entity)
        {
            if (entity == null) return null;

            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Portal = entity.Portal,
                Permissions = entity.Permissions?.Select(p => p.Permission.ToDto()).ToList() ?? new List<PermissionDto>()
            };
        }

        // Permission mappings
        public static PermissionDto ToDto(this Permission entity)
        {
            if (entity == null) return null;

            return new PermissionDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        // Audit mappings
        public static AuditDto ToDto(this Audit entity)
        {
            if (entity == null) return null;

            return new AuditDto
            {
                Id = entity.Id,
                User = entity.User,
                Action = entity.Action,
                When = entity.When,
                Ip = entity.Ip,
                Entity = entity.Entity,
                EntityId = entity.EntityId,
                Details = entity.Details
            };
        }

        // Assessment mappings
        public static AssessmentDto ToDto(this Assessment entity)
        {
            if (entity == null) return null;

            return new AssessmentDto
            {
                Id = entity.Id,
                OrganizationId = entity.OrganizationId,
                OrganizationName = entity.Organization?.Name ?? string.Empty,
                FrameworkVersionId = entity.FrameworkVersionId,
                FrameworkName = entity.FrameworkVersion?.Framework?.Name ?? string.Empty,
                FrameworkVersionName = entity.FrameworkVersion?.Name ?? string.Empty,
                Status = entity.Status,
                StartDate = entity.StartDate,
                CompletionDate = entity.CompletionDate,
                Notes = entity.Notes,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                TotalItems = entity.AssessmentItems?.Count(ai => !ai.Deleted) ?? 0,
                CompletedItems = entity.AssessmentItems?.Count(ai => !ai.Deleted && ai.Status != Core.Enums.ComplianceStatus.NOT_ASSESSED) ?? 0,
                ConformityItems = entity.AssessmentItems?.Count(ai => !ai.Deleted && ai.Status == Core.Enums.ComplianceStatus.CONFORMITY) ?? 0,
                NonConformityItems = entity.AssessmentItems?.Count(ai => !ai.Deleted && ai.Status == Core.Enums.ComplianceStatus.NON_CONFORMITY) ?? 0,
                ConformityWithNotesItems = entity.AssessmentItems?.Count(ai => !ai.Deleted && ai.Status == Core.Enums.ComplianceStatus.CONFORMITY_WITH_NOTES) ?? 0
            };
        }

        public static AssessmentListDto ToListDto(this Assessment entity)
        {
            if (entity == null) return null;

            var totalItems = entity.AssessmentItems?.Count(ai => !ai.Deleted) ?? 0;
            var completedItems = entity.AssessmentItems?.Count(ai => !ai.Deleted && ai.Status != Core.Enums.ComplianceStatus.NOT_ASSESSED) ?? 0;
            var progressPercentage = totalItems > 0 ? (int)Math.Round((double)completedItems / totalItems * 100) : 0;

            return new AssessmentListDto
            {
                Id = entity.Id,
                OrganizationId = entity.OrganizationId,
                OrganizationName = entity.Organization?.Name ?? string.Empty,
                FrameworkVersionId = entity.FrameworkVersionId,
                FrameworkName = entity.FrameworkVersion?.Framework?.Name ?? string.Empty,
                FrameworkVersionName = entity.FrameworkVersion?.Name ?? string.Empty,
                Status = entity.Status,
                StartDate = entity.StartDate,
                CompletionDate = entity.CompletionDate,
                CreationDate = entity.CreationDate,
                Progress = progressPercentage
            };
        }

        public static Assessment ToEntity(this CreateAssessmentDto dto)
        {
            if (dto == null) return null;

            return new Assessment
            {
                OrganizationId = dto.OrganizationId,
                FrameworkVersionId = dto.FrameworkVersionId,
                Status = Core.Enums.AssessmentStatus.DRAFT,
                StartDate = DateTime.UtcNow,
                Notes = dto.Notes,
                Deleted = false
            };
        }

        // AssessmentItem mappings
        public static AssessmentItemDto ToDto(this AssessmentItem entity)
        {
            if (entity == null) return null;

            return new AssessmentItemDto
            {
                Id = entity.Id,
                AssessmentId = entity.AssessmentId,
                ClauseId = entity.ClauseId,
                ClauseName = entity.Clause?.Name ?? string.Empty,
                Status = entity.Status,
                Notes = entity.Notes,
                CorrectiveActions = entity.CorrectiveActions,
                AssignedDepartmentId = entity.AssignedDepartmentId,
                AssignedDepartmentName = entity.AssignedDepartment?.Name,
                DueDate = entity.DueDate,
                CreationDate = entity.CreationDate,
                CreatedUser = entity.CreatedUser,
                LastModificationDate = entity.LastModificationDate,
                LastModificationUser = entity.LastModificationUser,
                Documents = entity.Documents?.Where(d => !d.Deleted).Select(d => d.ToDto()).ToList() ?? new List<AssessmentItemDocumentDto>(),
                CheckListItems = entity.CheckListItems?.Where(cl => !cl.Deleted).Select(cl => cl.ToDto()).ToList() ?? new List<AssessmentItemCheckListDto>()
            };
        }

        // AssessmentItemDocument mappings
        public static AssessmentItemDocumentDto ToDto(this AssessmentItemDocument entity)
        {
            if (entity == null) return null;

            return new AssessmentItemDocumentDto
            {
                Id = entity.Id,
                AssessmentItemId = entity.AssessmentItemId,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                UploadDate = entity.UploadDate,
                DocumentType = entity.DocumentType,
                ReleaseDate = entity.ReleaseDate,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.Department?.Name
            };
        }

        // AssessmentItemCheckList mappings
        public static AssessmentItemCheckListDto ToDto(this AssessmentItemCheckList entity)
        {
            if (entity == null) return null;

            return new AssessmentItemCheckListDto
            {
                Id = entity.Id,
                AssessmentItemId = entity.AssessmentItemId,
                CheckListId = entity.CheckListId,
                CheckListName = entity.CheckList?.Name ?? string.Empty,
                IsChecked = entity.IsChecked,
                Notes = entity.Notes
            };
        }

        // Utility methods for lists
        public static List<TDto> ToDtoList<TEntity, TDto>(this IEnumerable<TEntity> entities, Func<TEntity, TDto> mapper)
        {
            if (entities == null) return new List<TDto>();

            return entities.Select(mapper).ToList();
        }
    }
}