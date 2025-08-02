using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using WebBanHangOnline.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;
using Minio;
using Minio.DataModel.Args;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration; // Thêm IConfiguration để đọc cấu hình

        public AdminProductController(ApplicationDbContext context, IMinioClient minioClient, IConfiguration configuration)
        {
            _context = context;
            _minioClient = minioClient;
            _configuration = configuration; // Khởi tạo IConfiguration
        }

        // GET: Admin/AdminProduct
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Admin/AdminProduct/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,Price,Stock,CategoryId")] Product product,
            IFormFile? imageFile)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    product.ImageUrl = await SaveImageAndSetPolicy(imageFile);
                }
                else
                {
                    product.ImageUrl = "/images/default-product.png";
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm sản phẩm mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }


        // GET: Admin/AdminProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/AdminProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Description,Price,Stock,CategoryId,ImageUrl")] Product productFromForm,
            IFormFile? imageFile)
        {
            if (id != productFromForm.Id) return NotFound();

            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                var productToUpdate = await _context.Products.FindAsync(id);
                if (productToUpdate == null) return NotFound();

                if (imageFile != null)
                {
                    await DeleteImage(productToUpdate.ImageUrl);
                    productToUpdate.ImageUrl = await SaveImageAndSetPolicy(imageFile);
                }

                productToUpdate.Name = productFromForm.Name;
                productToUpdate.Description = productFromForm.Description;
                productToUpdate.Price = productFromForm.Price;
                productToUpdate.Stock = productFromForm.Stock;
                productToUpdate.CategoryId = productFromForm.CategoryId;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == productToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", productFromForm.CategoryId);
            return View(productFromForm);
        }


        // GET: Admin/AdminProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Admin/AdminProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                await DeleteImage(product.ImageUrl);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // =================================================================
        // HÀM XỬ LÝ ẢNH ĐÃ ĐƯỢC CẬP NHẬT ĐỂ TỰ ĐỘNG SET POLICY
        // =================================================================

        private async Task<string> SaveImageAndSetPolicy(IFormFile imageFile)
        {
            var bucketName = "products";
            var objectName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            // 1. Kiểm tra xem bucket đã tồn tại chưa
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                // Nếu chưa có, tạo bucket mới
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);

                // *** BẮT ĐẦU PHẦN QUAN TRỌNG: TỰ ĐỘNG ĐẶT POLICY ***
                // Tạo một policy JSON cho phép đọc công khai
                var policyJson = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"":[""*""]}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                        }}
                    ]
                }}";
                // Đặt policy cho bucket vừa tạo
                var spArgs = new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policyJson);
                await _minioClient.SetPolicyAsync(spArgs);
                Console.WriteLine($"Bucket '{bucketName}' created and policy set to public read.");
                // *** KẾT THÚC PHẦN QUAN TRỌNG ***
            }

            // 2. Upload file lên MinIO
            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(imageFile.OpenReadStream())
                    .WithObjectSize(imageFile.Length)
                    .WithContentType(imageFile.ContentType)
            );

            // 3. Trả về URL của ảnh
            var publicEndpoint = _configuration["Minio:PublicEndpoint"] ?? "localhost:9000";
            return $"http://{publicEndpoint}/{bucketName}/{objectName}";
        }

        private async Task DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.Contains("default-product.png"))
            {
                return;
            }

            try
            {
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Trim('/').Split('/');
                if (pathSegments.Length < 2) return;

                var bucketName = pathSegments[0];
                var objectName = string.Join("/", pathSegments.Skip(1));

                var args = new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectName);
                await _minioClient.RemoveObjectAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image from MinIO: {imageUrl}. Exception: {ex.Message}");
            }
        }
    }
}
