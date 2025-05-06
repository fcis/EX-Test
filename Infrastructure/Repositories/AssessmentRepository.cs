using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AssessmentRepository : GenericRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Assessment?> GetAssessmentWithItemsAsync(long assessmentId)
        {
            return await _dbContext.Set<Assessment>()
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.Organization)
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.Framework)
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.FrameworkVersion)
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted))
                    .ThenInclude(ai => ai.Clause)
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted))
                    .ThenInclude(ai => ai.AssignedDepartment)
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted))
                    .ThenInclude(ai => ai.Documents.Where(d => !d.Deleted))
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted))
                    .ThenInclude(ai => ai.CheckListItems.Where(cl => !cl.Deleted))
                        .ThenInclude(cl => cl.CheckList)
                .SingleOrDefaultAsync(a => a.Id == assessmentId && !a.Deleted);
        }

        public async Task<Assessment?> GetAssessmentByOrganizationAndFrameworkVersionAsync(long organizationId, long frameworkVersionId)
        {
            return await _dbContext.Set<Assessment>()

                .Where(a => a.OrganizationMembership.OrganizationId == organizationId &&
                       a.OrganizationMembership.FrameworkVersionId == frameworkVersionId &&
                       !a.Deleted)
                .OrderByDescending(a => a.CreationDate)
                .FirstOrDefaultAsync();
        }
        public async Task<PagedList<Assessment>> GetAssessmentsByOrganizationAsync(long organizationId, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<Assessment>()
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.Organization)
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.Framework)
                .Include(a => a.OrganizationMembership)
                    .ThenInclude(om => om.FrameworkVersion)
                .Where(a => a.OrganizationMembership.OrganizationId == organizationId && !a.Deleted);

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = pagingParameters.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.FrameworkVersion.Name.ToLower().Contains(searchTerm) ||
                    a.FrameworkVersion.Framework.Name.ToLower().Contains(searchTerm) ||
                    (a.Notes != null && a.Notes.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "frameworkname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.FrameworkVersion.Framework.Name) :
                            query.OrderBy(a => a.FrameworkVersion.Framework.Name);
                        break;
                    case "frameworkversionname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.FrameworkVersion.Name) :
                            query.OrderBy(a => a.FrameworkVersion.Name);
                        break;
                    case "status":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Status) :
                            query.OrderBy(a => a.Status);
                        break;
                    case "startdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.StartDate) :
                            query.OrderBy(a => a.StartDate);
                        break;
                    case "completiondate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.CompletionDate) :
                            query.OrderBy(a => a.CompletionDate);
                        break;
                    default:
                        query = query.OrderByDescending(a => a.CreationDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(a => a.CreationDate);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Assessment>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<PagedList<Assessment>> GetAssessmentsByStatusAsync(AssessmentStatus status, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<Assessment>()
                .Include(a => a.Organization)
                .Include(a => a.FrameworkVersion)
                    .ThenInclude(fv => fv.Framework)
                .Where(a => a.Status == status && !a.Deleted);

            // Apply search and sorting as in the previous method...

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Assessment>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<List<Assessment>> GetAssessmentsNeedingNotificationAsync()
        {
            // Get assessments with due dates coming up in the next 7 days
            var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);

            return await _dbContext.Set<Assessment>()
                .Include(a => a.Organization)
                .Include(a => a.FrameworkVersion)
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted && ai.DueDate.HasValue && ai.DueDate <= sevenDaysFromNow))
                    .ThenInclude(ai => ai.AssignedDepartment)
                .Where(a => a.Status == AssessmentStatus.IN_PROGRESS &&
                       !a.Deleted &&
                       a.AssessmentItems.Any(ai => !ai.Deleted &&
                                                 ai.DueDate.HasValue &&
                                                 ai.DueDate <= sevenDaysFromNow))
                .ToListAsync();
        }

        public async Task<int> GetAssessmentCompletionPercentageAsync(long assessmentId)
        {
            var assessment = await _dbContext.Set<Assessment>()
                .Include(a => a.AssessmentItems.Where(ai => !ai.Deleted))
                .SingleOrDefaultAsync(a => a.Id == assessmentId && !a.Deleted);

            if (assessment == null || !assessment.AssessmentItems.Any())
                return 0;

            var totalItems = assessment.AssessmentItems.Count;
            var assessedItems = assessment.AssessmentItems.Count(ai => ai.Status != ComplianceStatus.NOT_ASSESSED);

            return (int)Math.Round((double)assessedItems / totalItems * 100);
        }

        public async Task<IReadOnlyList<Assessment>> GetRecentAssessmentsAsync(int count)
        {
            return await _dbContext.Set<Assessment>()
                .Include(a => a.Organization)
                .Include(a => a.FrameworkVersion)
                    .ThenInclude(fv => fv.Framework)
                .Where(a => !a.Deleted)
                .OrderByDescending(a => a.CreationDate)
                .Take(count)
                .ToListAsync();
        }
    }

    public class AssessmentItemRepository : GenericRepository<AssessmentItem>, IAssessmentItemRepository
    {
        public AssessmentItemRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<AssessmentItem?> GetAssessmentItemWithDetailsAsync(long assessmentItemId)
        {
            return await _dbContext.Set<AssessmentItem>()
                .Include(ai => ai.Assessment)
                .Include(ai => ai.Clause)
                .Include(ai => ai.AssignedDepartment)
                .Include(ai => ai.Documents.Where(d => !d.Deleted))
                    .ThenInclude(d => d.Department)
                .Include(ai => ai.CheckListItems.Where(cl => !cl.Deleted))
                    .ThenInclude(cl => cl.CheckList)
                .SingleOrDefaultAsync(ai => ai.Id == assessmentItemId && !ai.Deleted);
        }

        public async Task<PagedList<AssessmentItem>> GetAssessmentItemsByAssessmentAsync(long assessmentId, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<AssessmentItem>()
                .Include(ai => ai.Clause)
                .Include(ai => ai.AssignedDepartment)
                .Where(ai => ai.AssessmentId == assessmentId && !ai.Deleted);

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = pagingParameters.SearchTerm.ToLower();
                query = query.Where(ai =>
                    ai.Clause.Name.ToLower().Contains(searchTerm) ||
                    (ai.Notes != null && ai.Notes.ToLower().Contains(searchTerm)) ||
                    (ai.CorrectiveActions != null && ai.CorrectiveActions.ToLower().Contains(searchTerm)) ||
                    (ai.AssignedDepartment != null && ai.AssignedDepartment.Name.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "clausename":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(ai => ai.Clause.Name) :
                            query.OrderBy(ai => ai.Clause.Name);
                        break;
                    case "status":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(ai => ai.Status) :
                            query.OrderBy(ai => ai.Status);
                        break;
                    case "department":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(ai => ai.AssignedDepartment != null ? ai.AssignedDepartment.Name : "") :
                            query.OrderBy(ai => ai.AssignedDepartment != null ? ai.AssignedDepartment.Name : "");
                        break;
                    case "duedate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(ai => ai.DueDate) :
                            query.OrderBy(ai => ai.DueDate);
                        break;
                    default:
                        query = query.OrderBy(ai => ai.Clause.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(ai => ai.Clause.Name);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<AssessmentItem>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<PagedList<AssessmentItem>> GetAssessmentItemsByStatusAsync(long assessmentId, ComplianceStatus status, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<AssessmentItem>()
                .Include(ai => ai.Clause)
                .Include(ai => ai.AssignedDepartment)
                .Where(ai => ai.AssessmentId == assessmentId && ai.Status == status && !ai.Deleted);

            // Apply search and sorting as in the previous method...

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<AssessmentItem>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<PagedList<AssessmentItem>> GetAssessmentItemsByDepartmentAsync(long assessmentId, long departmentId, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<AssessmentItem>()
                .Include(ai => ai.Clause)
                .Include(ai => ai.AssignedDepartment)
                .Where(ai => ai.AssessmentId == assessmentId && ai.AssignedDepartmentId == departmentId && !ai.Deleted);

            // Apply search and sorting as in the previous methods...

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<AssessmentItem>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<Dictionary<ComplianceStatus, int>> GetStatusSummaryAsync(long assessmentId)
        {
            var statusCounts = await _dbContext.Set<AssessmentItem>()
                .Where(ai => ai.AssessmentId == assessmentId && !ai.Deleted)
                .GroupBy(ai => ai.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<ComplianceStatus, int>();
            foreach (var status in Enum.GetValues(typeof(ComplianceStatus)).Cast<ComplianceStatus>())
            {
                var count = statusCounts.FirstOrDefault(sc => sc.Status == status)?.Count ?? 0;
                result.Add(status, count);
            }

            return result;
        }
    }

    public class AssessmentItemDocumentRepository : GenericRepository<AssessmentItemDocument>, IAssessmentItemDocumentRepository
    {
        public AssessmentItemDocumentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<AssessmentItemDocument>> GetDocumentsByAssessmentItemAsync(long assessmentItemId)
        {
            return await _dbContext.Set<AssessmentItemDocument>()
                .Include(d => d.Department)
                .Where(d => d.AssessmentItemId == assessmentItemId && !d.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<AssessmentItemDocument>> GetPagedDocumentsByAssessmentItemAsync(long assessmentItemId, PagingParameters pagingParameters)
        {
            var query = _dbContext.Set<AssessmentItemDocument>()
                .Include(d => d.Department)
                .Where(d => d.AssessmentItemId == assessmentItemId && !d.Deleted);

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = pagingParameters.SearchTerm.ToLower();
                query = query.Where(d =>
                    d.FileName.ToLower().Contains(searchTerm) ||
                    (d.DocumentType != null && d.DocumentType.ToLower().Contains(searchTerm)) ||
                    (d.Department != null && d.Department.Name.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "filename":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.FileName) :
                            query.OrderBy(d => d.FileName);
                        break;
                    case "uploaddate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.UploadDate) :
                            query.OrderBy(d => d.UploadDate);
                        break;
                    case "documenttype":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.DocumentType) :
                            query.OrderBy(d => d.DocumentType);
                        break;
                    case "department":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.Department != null ? d.Department.Name : "") :
                            query.OrderBy(d => d.Department != null ? d.Department.Name : "");
                        break;
                    default:
                        query = query.OrderByDescending(d => d.UploadDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(d => d.UploadDate);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<AssessmentItemDocument>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }

    public class AssessmentItemCheckListRepository : GenericRepository<AssessmentItemCheckList>, IAssessmentItemCheckListRepository
    {
        public AssessmentItemCheckListRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<AssessmentItemCheckList>> GetCheckListItemsByAssessmentItemAsync(long assessmentItemId)
        {
            return await _dbContext.Set<AssessmentItemCheckList>()
                .Include(cl => cl.CheckList)
                .Where(cl => cl.AssessmentItemId == assessmentItemId && !cl.Deleted)
                .ToListAsync();
        }

        public async Task<AssessmentItemCheckList?> GetByAssessmentItemAndCheckListAsync(long assessmentItemId, long checkListId)
        {
            return await _dbContext.Set<AssessmentItemCheckList>()
                .Include(cl => cl.CheckList)
                .SingleOrDefaultAsync(cl => cl.AssessmentItemId == assessmentItemId &&
                                       cl.CheckListId == checkListId &&
                                       !cl.Deleted);
        }
    }
}