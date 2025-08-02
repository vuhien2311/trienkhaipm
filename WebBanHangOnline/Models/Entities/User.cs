using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBanHangOnline.Models
{
    public class User : IdentityUser<int>
    {

        [StringLength(100)]
        public string? FullName { get; set; }

        public string? Address { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}