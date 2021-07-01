using BancDelTemps.ApiRest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest
{
    public class Context:DbContext
    {    
        public Context([NotNull] DbContextOptions options) : base(options) { }

        protected Context() : base() { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //User
            modelBuilder.Entity<User>().HasOne(u => u.Validator).WithMany(u => u.Validated);
            modelBuilder.Entity<User>().Navigation(u => u.Validator).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Validated).UsePropertyAccessMode(PropertyAccessMode.Property);

        }

        public User GetUser([NotNull]User user)
        {
            return GetUser(user.Email);
        }
        public User GetUser([NotNull]string email)
        {
            return Users.Where(u => u.Email.Equals(email)).FirstOrDefault();
        }
        public bool ExistUser([NotNull] User user)
        {
            return !Equals(GetUser(user), default);
        }
        public bool ExistUser([NotNull] string email)
        {
            return !Equals(GetUser(email), default);
        }
    }
}
