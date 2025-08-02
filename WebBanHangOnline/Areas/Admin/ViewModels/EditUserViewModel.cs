using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebBanHangOnline.Areas.Admin.ViewModels
{
    /// <summary>
    /// ViewModel này dùng để quản lý việc chỉnh sửa vai trò của người dùng.
    /// </summary>
    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        // Danh sách tất cả các vai trò có trong hệ thống
        public List<SelectListItem> AllRoles { get; set; }

        // Danh sách các vai trò mà người dùng hiện đang có
        public IList<string> UserRoles { get; set; }

        public EditUserViewModel()
        {
            AllRoles = new List<SelectListItem>();
            UserRoles = new List<string>();
        }
    }
}
