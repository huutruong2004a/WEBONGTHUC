using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class UserManagementModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class UserViewModel
        {
            public required string Id { get; set; }
            public required string Email { get; set; }
            public required string Role { get; set; }
        }

        public List<UserViewModel> Users { get; set; }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            Users = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }
        }

        public async Task<IActionResult> OnPostMakeAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Ki?m tra và t?o role Admin n?u ch?a t?n t?i
                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                }

                // Xóa role c? và thêm role Admin
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, SD.Role_Admin);
            }

            return RedirectToPage();
        }
    }
}