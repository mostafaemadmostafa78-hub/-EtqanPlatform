using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETQAN_BY_API.Services
{
    public class ArtisanService : IArtisanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public ArtisanService(ApplicationDbContext context, IFileService fileService, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // 1. عرض تفاصيل الحرفي
        public async Task<ArtisanDetailsDto> GetArtisanDetailsAsync(string userId)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.Portfolio)
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return null;

            return new ArtisanDetailsDto
            {
                Id = artisan.ApplicationUserId,
                FullName = artisan.User.FullName,
                BirthDate = artisan.BirthDate?? "",
                JobName = artisan.Job?.Name ?? "غير محدد",
                Bio = artisan.Bio,
                ExperienceYears = artisan.ExperienceYears,
                ProfilePicture = artisan.User.ProfilePicture,
                CoverPicture = artisan.CoverPicture ?? "/images/covers/default.jpg",
                WorkHours = artisan.WorkHours,
                ServiceArea = artisan.ServiceArea,
                ResponseTime = artisan.ResponseTime,
                IsEmergencyAvailable = artisan.IsEmergencyAvailable,
                PortfolioImages = artisan.Portfolio.Select(p => p.ImageUrl).ToList(),
                Rating = artisan.Reviews.Any() ? (double)Math.Round((decimal)artisan.Reviews.Average(r => r.Rating), 1) : 0,
                JoinedDate = artisan.User.CreatedAt,
                CompletedOrdersCount = artisan.Reviews.Count(),
                Governorate = $"{artisan.User.Governorate} - {artisan.ServiceArea}"
            };
        }

        // 2. التحديث الشامل لبيانات الحرفي
        public async Task<bool> UpdateArtisanProfileAsync(string userId, UpdateArtisanProfileDto dto)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return false;

            if (!string.IsNullOrEmpty(dto.BirthDate) && DateTime.TryParse(dto.BirthDate, out var birthDate))
            {
                var minAllowedDate = DateTime.Today.AddYears(-18);
                if (birthDate > minAllowedDate)
                    throw new Exception("عذراً، يجب أن يكون عمر الحرفي 18 عاماً على الأقل.");

                artisan.BirthDate = dto.BirthDate; // تحديث التاريخ بعد التأكد من السن
            }


            if (!string.IsNullOrEmpty(dto.FullName))
                artisan.User.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.Email))
                artisan.User.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                artisan.User.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Governorate))
                artisan.User.Governorate = dto.Governorate;

            if (dto.MaritalStatus.HasValue)
                artisan.MaritalStatus = (MaritalStatus)dto.MaritalStatus.Value;

            if (dto.ExperienceYears.HasValue)
                artisan.ExperienceYears = dto.ExperienceYears.Value;

            if (!string.IsNullOrEmpty(dto.ServiceArea))
                artisan.ServiceArea = dto.ServiceArea;

            if (!string.IsNullOrEmpty(dto.WorkHours))
                artisan.WorkHours = dto.WorkHours;

            if (!string.IsNullOrEmpty(dto.ResponseTime))
                artisan.ResponseTime = dto.ResponseTime;

            if (dto.IsEmergencyAvailable.HasValue)
                artisan.IsEmergencyAvailable = dto.IsEmergencyAvailable.Value;

            if (!string.IsNullOrEmpty(dto.Services))
                artisan.Services = dto.Services;

            // 3. معالجة النبذة الشخصية Bio
            if (!string.IsNullOrEmpty(dto.Bio))
            {
                string pattern = @"(\+?\d{10,14}|[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})";
                artisan.Bio = System.Text.RegularExpressions.Regex.Replace(dto.Bio, pattern, "[بيانات مخفية]");
            }
            if (dto.ProfilePic != null)
            {
                if (!string.IsNullOrEmpty(artisan.User.ProfilePicture))
                    _fileService.DeleteImage(artisan.User.ProfilePicture);

                artisan.User.ProfilePicture = await _fileService.UploadImageAsync(dto.ProfilePic, "uploads/profiles/artisans");
            }

            if (dto.CoverPic != null)
            {
                if (!string.IsNullOrEmpty(artisan.CoverPicture))
                    _fileService.DeleteImage(artisan.CoverPicture);

                artisan.CoverPicture = await _fileService.UploadImageAsync(dto.CoverPic, "images/covers/artisans");
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // 3. إضافة صورة لمعرض الأعمال
        public async Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            string imageUrl = await _fileService.UploadImageAsync(dto.Image, "portfolio");
            if (string.IsNullOrEmpty(imageUrl)) return false;

            var newImage = new ArtisanPortfolio
            {
                ArtisanId = artisan.Id,
                ImageUrl = imageUrl,
                Description = dto.Description
            };

            _context.ArtisanPortfolios.Add(newImage);
            return await _context.SaveChangesAsync() > 0;
        }

        // 4. عرض طلبات الحرفي
        public async Task<List<ArtisanOrderDto>> GetArtisanOrdersAsync(string artisanId, string? status = null)
        {

            var filtered = await _context.ServiceRequests
                .Include(r => r.Client).ThenInclude(c => c.User)
                .Where(r => r.ArtisanId == artisanId)  
                .ToListAsync();

            return filtered.Select(r => new ArtisanOrderDto
            {
                OrderId = r.Id,
                ClientName = r.Client?.User?.FullName ?? "عميل غير معروف",
                Location = r.Client?.User?.Governorate ?? "عنوان غير محدد",
                OrderDate = r.RequestDate.ToString("yyyy-MM-dd"),
                ServiceName = r.ServiceName,
                Status = r.Status.ToString()
            }).ToList();
        }

        // 5. تحديث حالة الطلب
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var request = await _context.ServiceRequests.FindAsync(orderId);
            if (request == null) return false;

            if (Enum.TryParse<RequestStatus>(newStatus, true, out var statusEnum))
            {
                request.Status = statusEnum;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        // 6. حذف الحرفي
        public async Task<bool> DeleteArtisanAccountAsync(string userId)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            artisan.IsDeleted = true;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 7. حذف صورة من المعرض
        public async Task<bool> DeleteImageFromPortfolioAsync(int imageId)
        {
            var image = await _context.ArtisanPortfolios.FindAsync(imageId);
            if (image == null) return false;

            _fileService.DeleteImage(image.ImageUrl);
            _context.ArtisanPortfolios.Remove(image);
            return await _context.SaveChangesAsync() > 0;
        }

        // 8. إضافة أو تحديث تقييم
        public async Task<bool> AddOrUpdateArtisanReviewAsync(string clientId, ArtisanReviewCreateDto dto)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ApplicationUserId == clientId);
            if (client == null) return false;

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.ClientId == clientId && o.ArtisanId != null);

            if (serviceRequest == null) return false;

            if (serviceRequest.Status != RequestStatus.Finished)
            {
                throw new Exception("يجب إنهاء الخدمة أولاً قبل التقييم.");
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ServiceRequestId == dto.OrderId && r.ArtisanId != null);

            if (existingReview != null)
            {
                if ((DateTime.Now - existingReview.CreatedAt).TotalHours > 24)
                {
                    throw new Exception("انتهت مهلة الـ 24 ساعة لتعديل التقييم.");
                }
                existingReview.Rating = dto.Rating;
                existingReview.Comment = dto.Comment;
                existingReview.CreatedAt = DateTime.Now;
            }
            else
            {
                var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == serviceRequest.ArtisanId);
                if (artisan == null) return false;

                _context.Reviews.Add(new Review
                {
                    ServiceRequestId = serviceRequest.Id,
                    ArtisanId = artisan.Id,
                    ReviewerId = clientId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.Now,
                    OrderId = _context.Orders.FirstOrDefault(o => o.ServiceRequestId == serviceRequest.Id)?.Id
                });
            }

            var saved = await _context.SaveChangesAsync() > 0;
            if (saved)
            {
                var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == serviceRequest.ArtisanId);
                if (artisan != null) await UpdateArtisanAverageRating(artisan.Id);
            }
            return saved;
        }

        private async Task UpdateArtisanAverageRating(int artisanId)
        {
            var artisan = await _context.Artisans
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(a => a.Id == artisanId);

            if (artisan != null)
            {
                artisan.AverageRating = artisan.Reviews.Any()
                    ? (decimal)Math.Round(artisan.Reviews.Average(r => (double)r.Rating), 1)
                    : 0;

                artisan.CompletedOrdersCount = await _context.ServiceRequests
                    .CountAsync(r => r.ArtisanId == artisan.ApplicationUserId && r.Status == RequestStatus.Finished);

                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> CreateInvoiceFromRequestAsync(IssueInvoiceDto dto)
        {
            var request = await _context.ServiceRequests.FirstOrDefaultAsync(r => r.Id == dto.ServiceRequestId);

            // 1. التحقق من وجود الطلب
            if (request == null) throw new Exception("الطلب غير موجود.");

            // 2.  منع العمل على طلب غير مقبول
            if (request.Status != RequestStatus.Accepted)
                throw new Exception($"لا يمكن إصدار فاتورة لطلب حالته {request.Status}. يجب قبول الطلب أولاً.");

            decimal total = dto.Items.Sum(i => i.Price * i.Quantity);

            // 3. جلب بيانات المستخدم
            var clientUserGuid = await _context.Clients
                .Where(c => c.ApplicationUserId == request.ClientId)
                .Select(c => c.ApplicationUserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(clientUserGuid))
                throw new Exception("خطأ في بيانات العميل: العميل غير مرتبط بحساب مستخدم (ApplicationUser).");

            var invoice = new Order
            {
                ApplicationUserId = clientUserGuid,
                ArtisanId = request.ArtisanId,
                ServiceRequestId = request.Id,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                IsServiceOrder = true,
                PaymentMethod = PaymentMethod.Cash
            };

            _context.Orders.Add(invoice);

            // 4. تغيير الحالة لـ Finished 
            request.Status = RequestStatus.Finished;

            try
            {
                await _context.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = invoice.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        ServiceName = item.Name,
                        TotalPrice = item.Price * item.Quantity
                    });
                }

                await _context.SaveChangesAsync();
                
                await _notificationService.SendNotificationAsync(
                    clientUserGuid,
                    "فاتورة جديدة",
                    $"تم إصدار فاتورة لطلبك بمبلغ {total} جنيهاً.",
                    "Order",
                    $"/orders/details/{invoice.Id}"
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء حفظ بيانات الفاتورة: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}