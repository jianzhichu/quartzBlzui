using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Blazor.ServerRender
{
    public class DocsDbContext : IdentityDbContext
    {
        public DocsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
