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
            public const string OrganizationAdmin = "OrganizationAdmin";
            public const string OrganizationUser = "OrganizationUser";
        }

        public static class Permissions
        {
            public const string ViewFrameworks = "ViewFrameworks";
            public const string CreateFramework = "CreateFramework";
            public const string EditFramework = "EditFramework";
            public const string DeleteFramework = "DeleteFramework";

            public const string ViewOrganizations = "ViewOrganizations";
            public const string CreateOrganization = "CreateOrganization";
            public const string EditOrganization = "EditOrganization";
            public const string DeleteOrganization = "DeleteOrganization";

            public const string ViewUsers = "ViewUsers";
            public const string CreateUser = "CreateUser";
            public const string EditUser = "EditUser";
            public const string DeleteUser = "DeleteUser";

            public const string ViewAudit = "ViewAudit";
        }
    }
}
