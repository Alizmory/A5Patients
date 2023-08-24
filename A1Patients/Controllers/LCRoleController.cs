using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A1Patients.Models;
using A1Patients.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace A1Patients.Controllers
{
    public class LCRoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> RoleManager;
        private readonly UserManager<IdentityUser> UserManager;

        public LCRoleController(RoleManager<IdentityRole> roleManager,
                                        UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.RoleManager = roleManager;
            this.UserManager = userManager;
            this._context = context;
        }
        public IActionResult Index()
        {
            var Roles = RoleManager.Roles.OrderBy(r => r.Name);
            return View(Roles);

        }
        [HttpPost, ActionName("CreatesRole")]
        public async Task<IActionResult> CreatesRole(string RoleName)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(RoleName) || string.IsNullOrWhiteSpace(RoleName))
                {
                    TempData["message"] = "the Role name is empty! please type";
                    return RedirectToAction("Index");
                }

                var resultRoleName = await RoleManager.FindByNameAsync(RoleName);

                if (resultRoleName != null)
                {
                    TempData["message"] = "This role already exist";
                    return RedirectToAction("Index");
                }

                try
                {
                    var role = new IdentityRole { Name = RoleName.Trim() };
                    var res = await RoleManager.CreateAsync(role);

                    // genrating message if it is successfull
                    if (res.Succeeded)
                    {
                        TempData["message"] = RoleName + ":" + "Successfully added";
                    }
                    else
                    {
                        // error messages
                        throw new Exception(res.Errors.FirstOrDefault().Description);
                    }
                }
                catch (Exception ex)
                {
                    TempData["message"] = $"the exception in creating {RoleName}: {ex.GetBaseException().Message}";
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet, ActionName("UsersRole")]
        public async Task<IActionResult> UsersRole(string roleID)
        {
            var role = await RoleManager.FindByIdAsync(roleID);
            var model = new List<UsersRole>();
            var userNotRole = new List<UsersRole>();

            if (role == null)
            {
                TempData["message"] = "This Role does not exist!";
                return RedirectToAction("Index");
            }
            else if (role != null)
            {
              
                ViewBag.RoleName = role.Name;

                foreach (var user in UserManager.Users)
                {
                    var userRoles = new UsersRole
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email
                    };

                    if (await UserManager.IsInRoleAsync(user, role.Name))
                    {
                        model.Add(userRoles);
                    }
                    else
                    {
                        userNotRole.Add(userRoles);
                    }
                }

                List<SelectListItem> selectedUser = userNotRole.ConvertAll(u =>
                {
                    return new SelectListItem()
                    {
                        Value = u.UserId,
                        Text = u.Username,
                    };
                });

                ViewData["UsersNotInRole"] = selectedUser.OrderBy(u => u.Text).ToList();
                ViewData["RoleID"] = roleID;
            }


            return View(model.OrderBy(u => u.Username).ToList());
        }

        [HttpPost, ActionName("AddUserRole")]
        public async Task<IActionResult> AddUserRole(string UserID, string RoleID)
        {
            if (ModelState.IsValid)
            {
                var role = await RoleManager.FindByIdAsync(RoleID);

                if (role == null)
                {
                    ModelState.AddModelError("", "this role does not exist");
                    return BadRequest(ModelState);
                }

                try
                {
                    var user = await UserManager.FindByIdAsync(UserID);
                    var result = await UserManager.AddToRoleAsync(user, role.Name);

                    if (result.Succeeded)
                    {
                        TempData["message"] = "The username was successfully added";
                    }
                    else
                    {
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                    }
                }
                catch (Exception ex)
                {
                    TempData["message"] = $"exception in adding the user to the role {role.Name}: {ex.GetBaseException().Message}";
                }
            }
            return RedirectToAction("UsersRole", new { roleID = RoleID });
        }

        public async Task<IActionResult> RemoveUserRole(string UserID, string RoleID, string RoleName)
        {
            if (RoleName.Equals("administrators"))
            {
                var activeUser = UserManager.GetUserId(HttpContext.User);

                if (UserID == activeUser)
                {
                    TempData["message"] = "active administrator user cannot be removed";
                    return RedirectToAction("UsersRole", new { roleID = RoleID });
                }
            }

            var user = await UserManager.FindByIdAsync(UserID);
            var RemovedUsers = await UserManager.RemoveFromRoleAsync(user, RoleName);

            if (RemovedUsers.Succeeded)
            {
                TempData["message"] = "the user removed successfully from the role";
            }
            else
            {
                TempData["message"] = RemovedUsers.Errors.FirstOrDefault().Description;
            }

            return RedirectToAction("UsersRole", new { roleID = RoleID });
        }

        [HttpGet, ActionName("DeletesRole")]
        public async Task<IActionResult> DeletesRole(string roleID)
        {
            var roleId = await RoleManager.FindByIdAsync(roleID);

            ViewBag.RoleName = roleId.Name;

            var deletemodel = new DeletesRole
            {
                RoleId = roleId.Id,
                RoleName = roleId.Name
            };

            var userCount = 0;

            foreach (var user in UserManager.Users)
            {
                if (await UserManager.IsInRoleAsync(user, roleId.Name))
                {
                    userCount++;

                    deletemodel.Users.Add(user.UserName);
                }
            }

            if (userCount == 0)
            {
                var result = await RoleManager.DeleteAsync(roleId);

                if (result.Succeeded)
                {
                    TempData["message"] = "The User role was successfully deleted";
                }

                return RedirectToAction("Index");
            }

            return View(deletemodel);
        }

        [HttpPost, ActionName("DeletesRole")]
        public async Task<IActionResult> DeletesRole(string RoleID, string deleteRole)
        {
            var roleId = await RoleManager.FindByIdAsync(RoleID);

            ViewBag.RoleName = roleId.Name;

            if (deleteRole.Equals("Cancel"))
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (roleId.Name.Equals("administrators"))
                {
                    TempData["message"] = "The administrator cannot be deleted!.";
                }
                else
                {
                    var result = await RoleManager.DeleteAsync(roleId);

                    if (result.Succeeded)
                    {
                        TempData["message"] = "User role deleted.";
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }

}