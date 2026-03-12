using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public CompanyAccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterCompanyDto dto)
        {
            if (dto.CommercialRegisterFile == null || dto.CommercialRegisterFile.Length == 0)
                return BadRequest("يرجى رفع ملف السجل التجاري");

            // 1. تحديد مسار الحفظ (مثلاً فولدر اسمه Uploads/Registers)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/registers");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // 2. إنشاء اسم فريد للملف عشان ميتكررش
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.CommercialRegisterFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 3. حفظ الملف على السيرفر
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.CommercialRegisterFile.CopyToAsync(stream);
            }

            // 4. إنشاء اليوزر وتخزين "مسار الملف" في الداتابيز
            var user = new ApplicationUser
            {
                FullName = dto.CompanyName,
                Email = dto.Email,
                UserName= dto.Email,
                CompanyName = dto.CompanyName,
                // بنخزن اللينك أو اسم الملف فقط في الداتابيز
                CommercialRegister = "/uploads/registers/" + fileName,
                UserType = UserType.Company
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            // ... باقي الكود الخاص بالـ Roles والرد
        
                if (result.Succeeded)
                {

                    // 3. إضافة الـ Role (تأكد إنها متكريتة في Program.cs زي ما عملنا)
                    await _userManager.AddToRoleAsync(user, "COMPANY");

                    return Ok(new { Message = "Company registered successfully" });
                }

                return BadRequest(result.Errors);
            }
        }
        //[HttpPost("register/company")]
        //public async Task<IActionResult> RegisterCompany([FromForm] RegisterCompanyDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var user = new ApplicationUser
        //        {
        //            FullName = dto.Email,
        //            Email = dto.Email,
        //            UserName = dto.Email,
        //            PhoneNumber = dto.PhoneNumber,
        //            UserType = UserType.Company
        //        };

        //        var result = await _userManager.CreateAsync(user, dto.Password);

        //        if (!result.Succeeded)
        //            return BadRequest(result.Errors);

        //        await _userManager.AddToRoleAsync(user, "Company");

        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/commercial_registers");
        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.CommercialRegisterFile.FileName;
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await dto.CommercialRegisterFile.CopyToAsync(fileStream);
        //        }

        //        _context.Companies.Add(new Company
        //        {
        //            ApplicationUserId = user.Id,
        //            CompanyName = dto.CompanyName,
        //            CommercialRegister = "/uploads/commercial_registers/" + uniqueFileName
        //        });

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new { message = "Company registered successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
        //    }
        //}
    
}