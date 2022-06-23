using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Data.Context
{
    public class DbContextIdentity : IdentityDbContext<ApplicationUser>
    {
        public DbContextIdentity(DbContextOptions<DbContextIdentity> options):base(options)
        {

        }
        public DbSet<Employees> Employees { get; set; }
    }
}
