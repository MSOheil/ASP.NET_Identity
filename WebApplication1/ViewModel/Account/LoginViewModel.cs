using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModel.Account
{
    public class LoginViewModel
    {
        //[Required,Display(Name = "نام کاربری")]
        //public string UserName { get; set; }


        [Required(ErrorMessage = "لطفا ایمیل خود را وارد کنید")]
        public string Email { get; set; }

        [Required, Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [ Display(Name = "مرابه خاطر بسپار")]
        public bool RememberMe { get; set; }


        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }


    }
}
