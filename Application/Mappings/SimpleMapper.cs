using Application.DTOs.Framework;
using Application.DTOs.Category;
using Application.DTOs.Clause;
using Application.DTOs.Organization;
using Application.DTOs.User;
using Application.DTOs.Audit;
using Core.Entities;
using Core.Entities.Identitiy;
using System.Collections.Generic;

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
                FrameworksCount = entity.Memberships?.Count(m => m.Status != Core.Enums.OrganizationMembershipStatus.DELETED) ?? 0
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

        // Utility methods for lists
        public static List<TDto> ToDtoList<TEntity, TDto>(this IEnumerable<TEntity> entities, Func<TEntity, TDto> mapper)
        {
            if (entities == null) return new List<TDto>();

            return entities.Select(mapper).ToList();
        }
    }
}