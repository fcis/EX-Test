using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        long? UserId { get; }
        string? UserName { get; }
        string? IpAddress { get; }
        bool IsAuthenticated { get; }
    }
}
