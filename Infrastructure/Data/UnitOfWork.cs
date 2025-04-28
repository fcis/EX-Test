using Core.Interfaces.Authentication;
using Core.Interfaces.Repositories;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IJwtTokenGenerator _jwtTokenGenerator;
    private IPasswordHasher _passwordHasher;
    private IDbContextTransaction? _transaction;

    private IAuditRepository _auditRepository;
    private ICategoryRepository _categoryRepository;
    private IClauseRepository _clauseRepository;
    private IClauseCheckListRepository _clauseCheckListRepository;
    private IFrameworkRepository _frameworkRepository;
    private IFrameworkVersionRepository _frameworkVersionRepository;
    private IFrameworkCategoriesRepository _frameworkCategoriesRepository;
    private IFwCatClausesRepository _fwCatClausesRepository;
    private IOrganizationRepository _organizationRepository;
    private IOrganizationDepartmentsRepository _organizationDepartmentsRepository;
    private IOrganizationMembershipRepository _organizationMembershipRepository;
    private IOrganizationClauseAnswersRepository _organizationClauseAnswersRepository;
    private IOrganizationCheckListAnswersRepository _organizationCheckListAnswersRepository;
    private IUserRepository _userRepository;
    private IRoleRepository _roleRepository;
    private IPermissionRepository _permissionRepository;
    private IRolePermissionsRepository _rolePermissionsRepository;

    // Add missing private fields for Assessment repositories
    private IAssessmentRepository _assessmentRepository;
    private IAssessmentItemRepository _assessmentItemRepository;
    private IAssessmentItemDocumentRepository _assessmentItemDocumentRepository;
    private IAssessmentItemCheckListRepository _assessmentItemCheckListRepository;

    public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger,
                    IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public IAuditRepository Audits => _auditRepository ??= new AuditRepository(_context);
    public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_context);
    public IClauseRepository Clauses => _clauseRepository ??= new ClauseRepository(_context);
    public IClauseCheckListRepository ClauseCheckLists => _clauseCheckListRepository ??= new ClauseCheckListRepository(_context);
    public IFrameworkRepository Frameworks => _frameworkRepository ??= new FrameworkRepository(_context);
    public IFrameworkVersionRepository FrameworkVersions => _frameworkVersionRepository ??= new FrameworkVersionRepository(_context);
    public IFrameworkCategoriesRepository FrameworkCategories => _frameworkCategoriesRepository ??= new FrameworkCategoriesRepository(_context);
    public IFwCatClausesRepository FwCatClauses => _fwCatClausesRepository ??= new FwCatClausesRepository(_context);
    public IOrganizationRepository Organizations => _organizationRepository ??= new OrganizationRepository(_context);
    public IOrganizationDepartmentsRepository OrganizationDepartments => _organizationDepartmentsRepository ??= new OrganizationDepartmentsRepository(_context);
    public IOrganizationMembershipRepository OrganizationMembershipRepository => _organizationMembershipRepository ??= new OrganizationMembershipRepository(_context);
    public IOrganizationClauseAnswersRepository OrganizationClauseAnswers => _organizationClauseAnswersRepository ??= new OrganizationClauseAnswersRepository(_context);
    public IOrganizationCheckListAnswersRepository OrganizationCheckListAnswers => _organizationCheckListAnswersRepository ??= new OrganizationCheckListAnswersRepository(_context);
    public IUserRepository Users => _userRepository ??= new UserRepository(_context, _jwtTokenGenerator, _passwordHasher);
    public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
    public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);
    public IRolePermissionsRepository RolePermissions => _rolePermissionsRepository ??= new RolePermissionsRepository(_context);

    // Add missing properties for Assessment repositories
    public IAssessmentRepository Assessments => _assessmentRepository ??= new AssessmentRepository(_context);
    public IAssessmentItemRepository AssessmentItems => _assessmentItemRepository ??= new AssessmentItemRepository(_context);
    public IAssessmentItemDocumentRepository AssessmentItemDocuments => _assessmentItemDocumentRepository ??= new AssessmentItemDocumentRepository(_context);
    public IAssessmentItemCheckListRepository AssessmentItemCheckLists => _assessmentItemCheckListRepository ??= new AssessmentItemCheckListRepository(_context);

    public async Task<int> CompleteAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SaveChangesAsync in UnitOfWork");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _transaction?.CommitAsync();
        }
        finally
        {
            _transaction?.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync();
        }
        finally
        {
            _transaction?.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}