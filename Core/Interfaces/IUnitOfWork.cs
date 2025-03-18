using Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository getters
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IFrameworkRepository Frameworks { get; }
        IFrameworkVersionRepository FrameworkVersions { get; }
        ICategoryRepository Categories { get; }
        IFrameworkCategoriesRepository FrameworkCategories { get; }
        IClauseRepository Clauses { get; }
        IFwCatClausesRepository FwCatClauses { get; }
        IClauseCheckListRepository ClauseCheckLists { get; }
        IOrganizationRepository Organizations { get; }
        IOrganizationDepartmentsRepository OrganizationDepartments { get; }
        IOrganizationMembershipRepository OrganizationMembershipRepository { get; }

        IOrganizationClauseAnswersRepository OrganizationClauseAnswers { get; }
        IOrganizationCheckListAnswersRepository OrganizationCheckListAnswers { get; }
        IRolePermissionsRepository RolePermissions { get; }
        IAuditRepository Audits { get; }

        // Transaction methods
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
