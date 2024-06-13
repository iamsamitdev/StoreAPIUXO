using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPIUXO.Data;
using StoreAPIUXO.Models;

namespace StoreAPIUXO.Controllers;

[Authorize] // ต้องเข้าสู่ระบบก่อนเข้าถึง Controller นี้
// [Authorize(Roles = UserRolesModel.Admin)] // ต้องเข้าสู่ระบบด้วยบทบาท Admin เท่านั้น
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ทดสอบเขียนฟังก์ชันการเชื่อมต่อ database
    // GET: /api/Category/testconnectdb
    [HttpGet("testconnectdb")]
    public void TestConnection()
    {
        // ถ้าเชื่อมต่อได้จะแสดงข้อความ Connection Success
        if (_context.Database.CanConnect())
        {
            Response.WriteAsync("Connection Success");
        }
        // ถ้าเชื่อมต่อไม่ได้จะแสดงข้อความ Connection Fail
        else
        {
            Response.WriteAsync("Connection Fail");
        }
    }

    // ฟังก์ชันสำหรับอ่านข้อมูลจาก category
    // GET: /api/Category
    [HttpGet]
    public ActionResult GetCategories()
    {
        // อ่านข้อมูลจากตาราง category ทั้งหมด
        var categories = _context.categories.ToList();
        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(categories);
    }

    // ฟังก์ชันสำหรับอ่านข้อมูลจาก category ตาม id
    // GET: /api/Category/1
    [HttpGet("{id}")]
    public ActionResult<category> GetCategory(int id)
    {
        // อ่านข้อมูลจากตาราง category ตาม id
        var category = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (category == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(category);
    }

    // ฟังก์ชันสำหรับเพิ่มข้อมูล category
    // POST: /api/Category
    [HttpPost]
    public ActionResult<category> AddCategory([FromBody] category category)
    {
        // เพิ่มข้อมูล category
        _context.categories.Add(category);
        // บันทึกข้อมูลลงฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(category);
    }

    // ฟังก์ชันสำหรับการแก้ไขข้อมูล category
    // PUT: /api/Category/1
    [HttpPut("{id}")]
    public ActionResult<category> UpdateCategory(int id, [FromBody] category category)
    {
        // ค้นหาข้อมูลจากตาราง Categories ตาม ID
        var cat = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (cat == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลในตาราง Categories
        cat.categoryname = category.categoryname;
        cat.categorystatus = category.categorystatus;
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(cat);
    }

    // ฟังก์ชันการลบข้อมูล category
    // DELETE: /api/Category/1
    [HttpDelete("{id}")]
    public ActionResult<category> DeleteCategory(int id)
    {
        // ค้นหาข้อมูลจากตาราง Categories ตาม ID
        var cat = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความว่าไม่พบข้อมูล
        if (cat == null)
        {
            return NotFound();
        }

        // ลบข้อมูลในตาราง Categories
        _context.categories.Remove(cat);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(cat);
    }

}