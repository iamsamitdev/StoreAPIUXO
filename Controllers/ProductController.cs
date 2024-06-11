using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPIUXO.Data;
using StoreAPIUXO.Models;

namespace StoreAPIUXO.Controllers;

// [Authorize] // ต้องเข้าสู่ระบบก่อนเข้าถึง Controller นี้
// [Authorize(Roles = UserRolesModel.Admin)] // ต้องเข้าสู่ระบบด้วยบทบาท Admin เท่านั้น
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Object สำหรับอ่าน path ของไฟล์
    private readonly IWebHostEnvironment _env;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public ProductController(
        ApplicationDbContext context,
        IWebHostEnvironment env
    )
    {
        _context = context;
        _env = env;
    }

    // ฟังก์ชันสำหรับอ่านข้อมูลจาก product
    // GET: /api/Product
    [HttpGet]
    public ActionResult GetProducts(
        [FromQuery] int page=1, 
        [FromQuery] int limit=100,
        [FromQuery] string? searchQuery=null,
        [FromQuery] int? selectedCategory=null
    )
    {
        int skip = (page - 1) * limit;

        // var products = _context.products.ToList();
        // อ่านข้อมูลจากตาราง products join กับ categories
        var query = _context.products.Join(
            _context.categories,
            p => p.categoryid,
            c => c.categoryid,
            (p, c) => new
            {
                p.productid,
                p.productname,
                p.unitprice,
                p.unitinstock,
                p.productpicture,
                p.createddate,
                p.modifieddate,
                p.categoryid,
                c.categoryname
            }
        );

        // กรณีมีการค้นหาข้อมูล
        if(!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(p => EF.Functions.ILike(p.productname!, $"%{searchQuery}%"));
        }

        // กรณีมีการค้นหาข้อมูลตาม category
        if(selectedCategory.HasValue){
            query = query.Where(p => p.categoryid == selectedCategory.Value);
        }

        // นับจำนวนข้อมูลทั้งหมด
        var totalRecords = query.Count();

        var products = query
                    .OrderByDescending(p => p.productid)
                    .Skip(skip)
                    .Take(limit)
                    .ToList();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(new {
            Total = totalRecords,
            Products = products
        });
    }

    // ฟังก์ชันสำหรับอ่านข้อมูลจาก product ตาม id
    // GET: /api/Product/1
    [HttpGet("{id}")]
    public ActionResult<product> GetProduct(int id)
    {
        // อ่านข้อมูลจากตาราง product ตาม id
        var product = _context.products.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (product == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(product);
    }

    // ฟังก์ชันสำหรับเพิ่มข้อมูล product
    // POST: /api/Product
    [HttpPost]
    public async Task<ActionResult<product>> AddProduct([FromForm] product product, IFormFile? image)
    {
        // เพิ่มข้อมูล product
        _context.products.Add(product);

        // ถ้ามีการอัพโหลดไฟล์
        if(image != null){
            // กำหนดชื่อไฟล์ภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // กำหนด path ที่จะบันทึกไฟล์
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่า path นี้มีอยู่หรือไม่ ถ้าไม่มีให้สร้าง path นี้
            if(!Directory.Exists(uploadPath)){
                Directory.CreateDirectory(uploadPath);
            }

            using(var fileStream = new FileStream(
                Path.Combine(uploadPath, fileName), 
                FileMode.Create)
            ){
                await image.CopyToAsync(fileStream);
            }

            // บันทึกชื่อไฟล์ภาพลงฐานข้อมูล
            product.productpicture = fileName;
        } else{
            product.productpicture = "noimg.jpg";
        }

        // บันทึกข้อมูลลงฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการแก้ไขข้อมูล product
    // PUT: /api/Product/1
    [HttpPut("{id}")]
    public async Task<ActionResult<product>> UpdateProduct(int id, [FromForm] product product, IFormFile? image)
    {
        // ค้นหาข้อมูลจากตาราง Products ตาม ID
        var prod = _context.products.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (prod == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลในตาราง Products
        prod.productname = product.productname;
        prod.unitprice = product.unitprice;
        prod.unitinstock = product.unitinstock;
        prod.categoryid = product.categoryid;
        prod.modifieddate = product.modifieddate;

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // กำหนด path ที่จะบันทึกไฟล์
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads มีหรือไม่
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // ลบไฟล์รูปภาพเดิม ถ้ามีการอัพโหลดรูปภาพใหม่ และรูปภาพเดิมไม่ใช่ noimg.jpg
            if(prod.productpicture != "noimg.jpg"){
                System.IO.File.Delete(Path.Combine(uploadPath, prod.productpicture!));
            }

            // บันทึกชื่อไฟล์รูปภาพลงในฐานข้อมูล
            prod.productpicture = fileName;
        }
    
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(prod);
    }

    // ฟังก์ชันการลบข้อมูล product
    // DELETE: /api/Product/1
    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id)
    {
        // ค้นหาข้อมูลจากตาราง Products ตาม ID
        var prod = _context.products.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (prod == null)
        {
            return NotFound();
        }

        // ตรวจสอบว่ามีไฟล์รูปภาพหรือไม่
        if(prod.productpicture != "noimg.jpg"){
            // กำหนด path ที่จะลบไฟล์
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            // ลบไฟล์รูปภาพ
            System.IO.File.Delete(Path.Combine(uploadPath, prod.productpicture!));
        }

        // ลบข้อมูลในตาราง Products
        _context.products.Remove(prod);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(prod);
    }

}