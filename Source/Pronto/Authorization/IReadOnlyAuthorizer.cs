using System;
using System.Collections.Generic;

namespace Pronto.Authorization
{
    public interface IReadOnlyAuthorizer
    {
        IEnumerable<Admin> Admins { get; }
        bool IsUserAdmin(string id);
    }
}