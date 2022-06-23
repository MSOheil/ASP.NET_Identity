using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Data.Context;
using WebApplication1.Models;
using WebApplication1.ViewModel;
using WebApplication1.ViewModel.Account;
using WebApplication1.ViewModel.Claims;
using WebApplication1.ViewModel.Roles;

namespace WebApplication1.Controllers
{
    //[Authorize(Policy  = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {

        private readonly RoleManager<IdentityRole> roleManager;

        public readonly UserManager<ApplicationUser> userManager;

        private readonly DbContextIdentity _context;
        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, DbContextIdentity context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            _context = context;
        }
       
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if(user is null)
            {
                ViewBag.ErrorMessage = $"User With Id :{userId} cannot Be Found ";
                return View("Not Found");
            }
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            foreach (var claims in ClaimsStor.AllClaims)
            {
                var userClaim = new UserClaims()
                {
                    ClaimType = claims.Type
                };

                if(existingUserClaims.Any(a=>a.Type== claims.Type && a.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);

            }
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user is null)
            {
                ViewBag.ErrorMessage = $"User With Id :{model.UserId} cannot Be Found ";
                return View("Not Found");
            }
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Remove user Exisiting claims");
                return View(model);
            }
            result = await userManager.AddClaimsAsync(user, model.Claims.Where(a => a.IsSelected)
                .Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));
            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Can not add Selected Claim To User");
                return View(model);
            }


            return RedirectToAction("EditUser", new { Id = model.UserId });



        }

        [HttpGet]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);

            if(user is null)
            {
                ViewBag.ErrorMessage = $"User With Id ={userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();
            var roles = roleManager.Roles.ToList();
          await  _context.DisposeAsync();
            foreach (var role in roles)
            {
                var userRoleViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                var userInRole = await userManager.IsInRoleAsync(user, role.Name);
                if(userInRole)
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);

        }

        [HttpPost]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model,string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);

            if (user is null) 
            {
                ViewBag.ErrorMessage = $"User With Id ={userId} cannot be found";
                return View("NotFound");
            }
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user,roles);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user Existing Role");
                return View(model);
            }
            result = await userManager.AddToRolesAsync(user, model.Where(a => a.IsSelected).Select(d => d.RoleName));
            return RedirectToAction("EditUser", new { Id = userId });
        }


        [HttpPost]
        [Authorize(Policy ="AdminMainRolePolicy")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user is null)
            {
                ViewBag.ErrorMessage = $"User With Id = {id} can Not Be Found";
                return View("NotFound");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);


                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Administration");
                }
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }

                return View("ListUsers");

            }

        }
        [HttpPost]
        [Authorize(Policy ="DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {id} can Not Be Found";
                return View("NotFound");
            }
            else
            {
                var result = await roleManager.DeleteAsync(role);


                if (result.Succeeded)
                {
                    return RedirectToAction("ListsRoles", "Administration");
                }
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }

                return View("ListsRoles");

            }

        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users.AsNoTracking();
            return View(users);
        }


        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityRole = new IdentityRole()
                {
                    Name = model.RoleName
                };
                var resultRoleCreate = await roleManager.CreateAsync(identityRole);
                if (resultRoleCreate.Succeeded)
                {
                    return RedirectToAction("ListsRoles", "Administration");
                }
                foreach (var err in resultRoleCreate.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ListsRoles()
        {
            var roles = roleManager.Roles.AsNoTracking();
            return View(roles);
        }
        [HttpGet]
        public async Task<IActionResult> EditRole(string Id)
        {
            var role = await roleManager.FindByIdAsync(Id);
            if (role is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {Id} can Not Be Found";
                return View("NotFound");
            }
            var model = new EditRoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
            };
            var Users = userManager.Users.AsNoTracking().ToList();
            await _context.DisposeAsync();
            foreach (var user in Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);


        }
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {model.Id} can Not Be Found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListsRoles");
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditUserInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {roleId} can Not Be Found";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();
            var Users = userManager.Users.AsNoTracking().ToList();
            await _context.DisposeAsync();
            foreach (var user in Users)
            {
                var userRoleViewModel = new UserRoleViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                var isRoleExistUser = await userManager.IsInRoleAsync(user, role.Name);
                if (isRoleExistUser)
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUserInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {roleId} can Not Be Found";
                return View("NotFound");
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }

            }
            return RedirectToAction("EditRole", new { Id = roleId });
        }
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if(user is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {id} can Not Be Found";
                return View("NotFound");
            }
            var userClaims = await userManager.GetClaimsAsync(user);
            var userRole = await userManager.GetRolesAsync(user);


            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = userClaims.Select(a => a.Type + " : " + a.Value).ToList(),
                Roles = userRole.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user is null)
            {
                ViewBag.ErrorMessage = $"Role With Id = {model.Id} can Not Be Found";
                return View("NotFound");
            }
           else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;

                var result = await userManager.UpdateAsync(user);

                if(result.Succeeded)
                {
                    RedirectToAction("ListsUsers");
                }
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                return View(model);
            }
        }
     

    }
}
