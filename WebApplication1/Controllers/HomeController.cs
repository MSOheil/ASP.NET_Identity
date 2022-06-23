using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Security;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IDataProtector protector;



        public HomeController(ILogger<HomeController> logger /*,*/ /*IDataProtectionProvider dataProtectionProvider,*/
        //    DataProtectionPurposeStrings dataProtectionPurposeStrings)
        )
        {
            _logger = logger;
            //protector = dataProtectionProvider
            //    .CreateProtector(dataProtectionPurposeStrings.EmployeeIdRoutValue);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
