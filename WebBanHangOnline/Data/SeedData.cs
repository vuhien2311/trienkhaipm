using Microsoft.AspNetCore.Identity;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Data
{
    public static class SeedData
    {
        // Phương thức này sẽ được gọi khi ứng dụng khởi chạy
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // --- Tạo các Roles (vai trò) mặc định ---
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                // Kiểm tra xem role đã tồn tại chưa
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Nếu chưa, tạo role mới
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // --- Tạo tài khoản Admin mặc định ---
            var adminEmail = "vuhien7789@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Kiểm tra xem tài khoản admin đã tồn tại chưa
            if (adminUser == null)
            {
                var newAdminUser = new User
                {
                    UserName = "admin_vuhien", // Tên đăng nhập là duy nhất
                    Email = adminEmail,
                    FullName = "Vũ Minh Hiển (Admin)",
                    EmailConfirmed = true // Xác thực email luôn để đăng nhập được ngay
                };

                // Tạo người dùng với mật khẩu "admin"
                // Lưu ý: Trong dự án thực tế, hãy dùng một mật khẩu mạnh hơn.
                var result = await userManager.CreateAsync(newAdminUser, "23112005");

                if (result.Succeeded)
                {
                    // Gán vai trò "Admin" cho người dùng vừa tạo
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
        }
    }
}
