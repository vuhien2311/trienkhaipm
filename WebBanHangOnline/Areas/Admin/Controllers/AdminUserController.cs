using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebBanHangOnline.Areas.Admin.ViewModels;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminUserController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/AdminUser
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<object>();
            foreach (User user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new { User = user, Roles = roles });
            }
            return View(userRolesViewModel);
        }

        // GET: Admin/AdminUser/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                UserId = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                UserRoles = userRoles,
                AllRoles = allRoles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/AdminUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            // Cập nhật vai trò
            var selectedRoles = model.UserRoles ?? new List<string>();
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể thêm vai trò cho người dùng.");
                return View(model);
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa vai trò cũ của người dùng.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật vai trò cho người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/AdminUser/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Admin/AdminUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn xảy ra khi xóa người dùng ID '{id}'.");
            }

            TempData["SuccessMessage"] = "Đã xóa người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
