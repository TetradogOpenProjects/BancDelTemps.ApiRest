using BancDelTemps.ApiRest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest
{
    public class Context : DbContext
    {
        public Context([NotNull] DbContextOptions options) : base(options) { }

        protected Context() : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UserPermiso> PermisosUsuarios { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<TransaccionDelegada> TransaccionesDelegadas { get; set; }
        public DbSet<Operacion> Operaciones { get; set; }
        public DbSet<Gift> Gifts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            #region User
            modelBuilder.Entity<User>().HasOne(u => u.Validator).WithMany(u => u.Validated);
            modelBuilder.Entity<User>().HasMany(u => u.Permisos).WithOne(up => up.User);
            modelBuilder.Entity<User>().HasMany(u => u.Granted).WithOne(up => up.GrantedBy);
            modelBuilder.Entity<User>().HasMany(u => u.Revoked).WithOne(up => up.RevokedBy);
            modelBuilder.Entity<User>().HasMany(u => u.TransaccionesIn).WithOne(up => up.UserTo);
            modelBuilder.Entity<User>().HasMany(u => u.TransaccionesFrom).WithOne(up => up.UserFrom);
            modelBuilder.Entity<User>().HasMany(u => u.TransaccionesSigned).WithOne(up => up.User);
            modelBuilder.Entity<User>().HasMany(u => u.TransaccionesValidator).WithOne(up => up.UserValidator);
            modelBuilder.Entity<User>().HasMany(u => u.OperacionesHechas).WithOne(o => o.User);
            modelBuilder.Entity<User>().HasMany(u => u.OperacionesRevisadas).WithOne(o => o.Revisor);

            modelBuilder.Entity<User>().HasMany(u => u.MessageFrom).WithOne(up => up.From);
            modelBuilder.Entity<User>().HasMany(u => u.MessageTo).WithOne(up => up.To);
            modelBuilder.Entity<User>().HasMany(u => u.MessageRevised).WithOne(up => up.Revisor);

            modelBuilder.Entity<User>().Navigation(u => u.Granted).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Revoked).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Permisos).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Validator).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.Validated).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.TransaccionesIn).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.TransaccionesFrom).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.TransaccionesSigned).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.TransaccionesValidator).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.OperacionesHechas).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<User>().Navigation(u => u.OperacionesRevisadas).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            #region Messages
            modelBuilder.Entity<Message>().Navigation(m => m.From).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Message>().Navigation(m => m.To).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Message>().Navigation(m => m.Revisor).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            #region Permisos
            modelBuilder.Entity<Permiso>().HasMany(u => u.Users).WithOne(up => up.Permiso);
            modelBuilder.Entity<Permiso>().Navigation(u => u.Users).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            #region UserPermisos
            modelBuilder.Entity<UserPermiso>().HasKey(nameof(UserPermiso.UserId), nameof(UserPermiso.PermisoId));
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.GrantedBy).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.RevokedBy).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.Permiso).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<UserPermiso>().Navigation(u => u.User).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            # region Transacciones
            modelBuilder.Entity<Transaccion>().HasOne(t => t.Operacion).WithMany();
            modelBuilder.Entity<Transaccion>().Navigation(u => u.Operacion).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Transaccion>().Navigation(u => u.UserFrom).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Transaccion>().Navigation(u => u.UserTo).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Transaccion>().Navigation(u => u.UserValidator).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            #region Transaccion delegada
            modelBuilder.Entity<TransaccionDelegada>().HasOne(t => t.Operacion).WithMany();
            modelBuilder.Entity<TransaccionDelegada>().Navigation(u => u.Operacion).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<TransaccionDelegada>().Navigation(u => u.User).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion
            #region Operacion
            modelBuilder.Entity<Operacion>().Navigation(o => o.Revisor).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Operacion>().Navigation(o => o.User).UsePropertyAccessMode(PropertyAccessMode.Property);
            #endregion

            #region Gifts

            modelBuilder.Entity<Gift>().Navigation(g=>g.Transaccion).UsePropertyAccessMode(PropertyAccessMode.Property);

            #endregion
            //añado datos al principio
            //SeedDataBase(modelBuilder);




        }
        

        public User GetUserPermiso([NotNull] User user)
        {
            return GetUserPermiso(user.Email);
        }
        public User GetUserPermiso([NotNull] string email)
        {
            return GetUsersPermisos().Where(u => u.Email.Equals(email))
                             .FirstOrDefault();
        }
        public User GetUserPermiso(long idUser)
        {
            return GetUsersPermisos().Where(u => u.Id == idUser).FirstOrDefault();
        }
        public IIncludableQueryable<User, Permiso> GetUsersPermisos()
        {
            return Users.Include(u => u.Permisos)
                        .ThenInclude(p => p.Permiso);


        }
        public IEnumerable<Gift> GetUserGifts([NotNull]User user){
            return GetUserGifts(user.Id);
        }
        public IEnumerable<Gift> GetUserGifts(long userId)
        {
            return Gifts.Include(g => g.Transaccion)
                        .Where(g => g.Transaccion.UserFromId == userId || g.Transaccion.UserToId == userId);
        }
        public User GetUserPermisoWithTransacciones([NotNull] User user)
        {
            return GetUserPermisoWithTransacciones(user.Email);
        }
        public User GetUserPermisoWithTransacciones([NotNull] string email)
        {
            email=email.ToLower();
            return GetUsersPermisosWithTransacciones().Where(u => u.Email.Equals(email))
                                                      .FirstOrDefault();
        }

        public Transaccion GetTransaccion([NotNull] Operacion operacion)
        {
            return Transacciones.Include(t=>t.Operacion).Where(t => t.OperacionId.Equals(operacion.Id)).FirstOrDefault();
        }
        public TransaccionDelegada GetTransaccionDelegada([NotNull] Operacion operacion)
        {
            return TransaccionesDelegadas.Where(t => t.OperacionId.Equals(operacion.Id)).FirstOrDefault();
        }
        public User GetUserPermisoWithTransacciones(long idUser)
        {
            return GetUsersPermisosWithTransacciones().Where(u => u.Id.Equals(idUser))
                                                      .FirstOrDefault();
        }
        public User GetFullUser([NotNull] string email)
        {
            //cuando esté todo desarrollado poner el filtro más grande
            return GetUserPermisoWithTransacciones(email);
        }
        public IIncludableQueryable<User, ICollection<Transaccion>> GetUsersPermisosWithTransacciones()
        {
            return GetUsersPermisos().Include(u => u.TransaccionesIn)
                                     .Include(u => u.TransaccionesFrom)
                                     .Include(u => u.TransaccionesSigned)
                                     .ThenInclude(t => t.Operacion)
                                     .Include(u => u.TransaccionesValidator);


        }
        public bool ExistUser([NotNull] User user)
        {
            return ExistUser(user.Email);
        }
        public bool ExistUser([NotNull] string email)
        {
            email=email.ToLower();
            return Users.Where(u => u.Email.Equals(email)).Any();
        }
        public bool ExistUser(long idUsuario)
        {
            return !Equals(Users.Find(idUsuario), default);
        }
    }
}
