using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProcessManagerCore.Models
{
    public class PmDbContext : IdentityDbContext
    {
        public PmDbContext(DbContextOptions<PmDbContext> options)
            : base(options)
        {
        }
    }
}
