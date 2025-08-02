using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHangOnline.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(250)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public int Stock { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}