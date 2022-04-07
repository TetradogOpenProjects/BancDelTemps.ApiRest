using BancDelTemps.ApiRest.Controllers;
using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BancDelTemps.ApiRest.Testing
{
    public class AccountControllerTest : TestBase
    {
        public AccountControllerTest() : base()
        {
            Controller = new AccountController(Context, Configuration);
            Controller.ContextoHttp = ContextoHttp;
        }
        public AccountController Controller { get; set; }
        #region GetUser la información del usuario que ha hecho login
        [Fact]
        public void GetUserInfoInvalidated()
        {
            DoAction(GetNoValidatedUser(), GetUserInfo);
        }
        [Fact]
        public void GetUserInfoValidated()
        {
            DoAction(GetValidatedUser(), GetUserInfo);
        }
        [Fact]
        public void GetUserInfoAdmin()
        {
            DoAction(GetUserWithPermiso(Permiso.ADMIN), GetUserInfo);
        }
        [Fact]
        public void GetUserInfoModValidator()
        {
            DoAction(GetUserWithPermiso(Permiso.MODVALIDATION), GetUserInfo);
        }
        [Fact]
        public void GetUserInfoCanListUser()
        {
            DoAction(GetUserWithPermiso(Permiso.CANLISTUSER), GetUserInfo);
        }
        [Fact]
        public void GetUserInfoAnonimo()
        {
            DoAction(default, GetUserInfo);
        }

        private async Task GetUserInfo(User user)
        {
            IActionResult result = Controller.GetUser();
            if (!Equals(user, default))
                Assert.IsType<OkObjectResult>(result);
            else Assert.IsType<ForbidResult>(result);
        }
        #endregion
        #region UpdateUser
        [Fact]
        public void UpdateUserInvalidated()
        {
            DoAction(GetNoValidatedUser(), UpdateUser);
        }
        [Fact]
        public void UpdateUserValidated()
        {
            DoAction(GetValidatedUser(), UpdateUser);
        }
        [Fact]
        public void UpdateUserAdmin()
        {
            DoAction(GetUserWithPermiso(Permiso.ADMIN), UpdateUser);
        }
        [Fact]
        public void UpdateUserModValidator()
        {
            DoAction(GetUserWithPermiso(Permiso.MODVALIDATION), UpdateUser);
        }
        [Fact]
        public void UpdateUserValidWithMod()
        {
            DoAction(GetUserWithPermiso(Permiso.MODUSER),GetValidatedUser(), UpdateUser);
        }
        [Fact]
        public void UpdateUserInvalidWithMod()
        {
            DoAction(GetUserWithPermiso(Permiso.MODUSER), GetNoValidatedUser(), UpdateUser);
        }
        [Fact]
        public void UpdateModWithMod()
        {
            DoAction(GetUserWithPermiso(Permiso.MODUSER), GetUserWithPermiso(Permiso.MODUSER), UpdateUser);
        }
        [Fact]
        public void UpdateModWithAdmin()
        {
            DoAction(GetUserWithPermiso(Permiso.ADMIN), GetUserWithPermiso(Permiso.MODUSER), UpdateUser);
        }
        private async Task UpdateUser(User user)
        {
           await UpdateUser(default, user);
            
        }
        private async Task UpdateUser(User mod, User user)
        {
            const string NEWEMAIL = "_new_EMAIL@googlemail.com";

            IActionResult result;
            UserDTO updated;
            UserDTO toUpdate = new UserDTO(user);

            toUpdate.Name = user.Name + "_updated";
            toUpdate.NewEmail =Seed.NextInt64()+ NEWEMAIL;
            result = await Controller.UpdateUser(toUpdate);

            if (result is OkResult)
            {
                UpdateContextUser(user);
                //de momento no pasa de aqui y en vez de dar error dice que la prueba se ha superado con éxito...
                updated = (Controller.GetUser() as OkObjectResult).Value as UserDTO;
                Assert.True(Equals(toUpdate.Name, updated.Name) && Equals(Equals(mod,default)? user.Email:toUpdate.Email, updated.Email));

            }
            else Assert.IsType<ForbidResult>(result);
        }
        #endregion
        #region Test Validar Cambio Email
        [Theory]
        [InlineData("gabriel.cat.developer@gmail.com")]
        [InlineData("tetradog@gmail.com")]
        public void ValidationEmailIsTrue(string email)
        {
            Assert.True(AccountController.ValidateEmail(email));
        }
        [Theory]
        [InlineData("gabriel.cat.developer@hotmail.com")]
        [InlineData(".gabriel.cat.developer@gmail.com")]
        [InlineData("@gmail.com")]
        public void ValidationEmailIsFalse(string email)
        {
            Assert.False(AccountController.ValidateEmail(email));
        }
        #endregion
    }
}
