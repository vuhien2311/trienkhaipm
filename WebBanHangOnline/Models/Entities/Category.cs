using System.ComponentModel.DataAnnotations;

namespace WebBanHangOnline.Models
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<Product> Products { get; set; }
    }
}