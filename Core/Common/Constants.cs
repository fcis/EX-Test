using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Auditor = "Auditor";
            public const string ComplianceManager = "OrganizationUser";
        }

        public static class Permissions
        {
            // Framework permissions
            public const string ViewFrameworks = "ViewFrameworks";
            public const string CreateFramework = "CreateFramework";
            public const string EditFramework = "EditFramework";
            public const string DeleteFramework = "DeleteFramework";

            // Organization permissions
            public const string ViewOrganizations = "ViewOrganizations";
            public const string CreateOrganization = "CreateOrganization";
            public const string EditOrganization = "EditOrganization";
            public const string DeleteOrganization = "DeleteOrganization";

            // User permissions
            public const string ViewUsers = "ViewUsers";
            public const string CreateUser = "CreateUser";
            public const string EditUser = "EditUser";
            public const string DeleteUser = "DeleteUser";

            // Audit permissions
            public const string ViewAudit = "ViewAudit";

            // Category permissions
            public const string ViewCategories = "ViewCategories";
            public const string CreateCategory = "CreateCategory";
            public const string EditCategory = "EditCategory";
            public const string DeleteCategory = "DeleteCategory";

            // Clause permissions
            public const string ViewClauses = "ViewClauses";
            public const string CreateClause = "CreateClause";
            public const string EditClause = "EditClause";
            public const string DeleteClause = "DeleteClause";
        }
    }
}
