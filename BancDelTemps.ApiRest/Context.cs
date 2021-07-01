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
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UserPermiso> PermisosUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //User
            modelBuilder.Entity<User>().HasOne(u => u.Validator).WithMany(u => u.Validated).HasForeignKey(u=>u.ValidatorId);
            modelBuilder.Entity<User>().HasMany(u => u.Permisos).WithOne(up => up.User).HasForeignKey(p=>p.UserId);
            modelBuilder.Entity<User>().HasMany(u => u.Granted).WithOne(up => up.GrantedBy).HasForeignKey(g=>g.GrantedById);
            modelBuilder.Entity<User>().HasMany(u => u.Revoked).WithOne(up => up.RevokedBy).HasForeignKey(r=>r.RevokedById);
            modelBuilder.Entity<User>().Navigation(u => u.Granted).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Revoked).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Permisos).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Validator).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Validated).UsePropertyAccessMode(PropertyAccessMode.Property);
            //Permisos
            modelBuilder.Entity<Permiso>().HasMany(u => u.Users).WithOne(up => up.Permiso).HasForeignKey(p=>p.PermisoId);
            modelBuilder.Entity<Permiso>().Navigation(u => u.Users).UsePropertyAccessMode(PropertyAccessMode.Property);
            //UserPermisos
            modelBuilder.Entity<UserPermiso>().HasKey(nameof(UserPermiso.UserId), nameof(UserPermiso.PermisoId));
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.GrantedBy).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.RevokedBy).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.Permiso).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.User).UsePropertyAccessMode(PropertyAccessMode.Property);



            //añado datos al principio
            modelBuilder.Entity<Permiso>().HasData(Permiso.Todos.Select(p => new Permiso() { Nombre = p }));
        }

        public User GetUser([NotNull]User user)
        {
            return GetUser(user.Email);
        }
        public User GetUser([NotNull]string email)
        {
            return Users.Include(u=>u.Permisos)
                        .ThenInclude(p=>p.Permiso)
                        .Where(u => u.Email.Equals(email))
                        .FirstOrDefault();
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
