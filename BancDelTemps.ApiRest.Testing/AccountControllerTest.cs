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

        private void GetUserInfo(User user)
        {
            IActionResult result = Controller.GetUser();
            if (!Equals(user, default))
                Assert.IsType<OkObjectResult>(result);
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
