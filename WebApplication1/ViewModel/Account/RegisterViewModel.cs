using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Utility;

namespace WebApplication1.ViewModel.Account
{
    public class RegisterViewModel
    {

        [Required]
        [Display(Name ="نام کاربری")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "ایمیل")]
        [EmailAddress]
        [Remote(action:"VeryfyEmail",controller:"Account")]
        [ValidEmailDomain(allowedDomain:"gmail.com",ErrorMessage ="Email Doamin Most Be Gmail.com")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "رمز عبور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "تکرار رمز عبور")]
        [Compare(nameof(Password),ErrorMessage ="Password And Confirm Password Is Not Match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "شهر محل سکونت")]
        public string City { get; set; }
    }
}
