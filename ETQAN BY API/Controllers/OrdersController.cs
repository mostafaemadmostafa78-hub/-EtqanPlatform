using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public OrdersController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        #region جزء المتجر

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder(OrderRequestDto dto)
        {
            var user = await _context.Users.AnyAsync(u => u.Id == dto.ApplicationUserId);
            if (!user) return BadRequest("User not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order { ApplicationUserId = dto.ApplicationUserId, OrderDate = DateTime.Now, TotalPrice = 0, OrderItems = new List<OrderItem>() };
                decimal finalPrice = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity) return BadRequest("Stock Issue");

                    var orderDetail = new OrderItem { ProductId = product.Id, Quantity = item.Quantity, UnitPrice = product.Price, Order = order };
                    product.StockQuantity -= item.Quantity;
                    finalPrice += (product.Price * item.Quantity);
                    order.OrderItems.Add(orderDetail);
                }

                order.TotalPrice = finalPrice;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { OrderId = order.Id, Total = order.TotalPrice });
            }
            catch { await transaction.RollbackAsync(); return StatusCode(500, "Error"); }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] List<CartItemDto> cartItems)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order { ApplicationUserId = userId, OrderDate = DateTime.Now, TotalPrice = 0 };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.Id);
                    if (product == null || product.StockQuantity < item.Quantity) return BadRequest();
                    _context.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = product.Id, Quantity = item.Quantity, UnitPrice = product.Price });
                    product.StockQuantity -= item.Quantity;
                    total += (product.Price * item.Quantity);
                }
                order.TotalPrice = total;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { orderId = order.Id, total });
            }
            catch { await transaction.RollbackAsync(); return BadRequest(); }
        }

        [HttpGet("store-order/{id}")]
        public async Task<ActionResult> GetStoreOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems!).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        #endregion
        [Authorize(Roles = "Client")]
        [HttpPost("create-service-request")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ApplicationUserId == dto.ClientId);
            if (client == null) return BadRequest("العميل غير موجود");

            if (dto.CompanyId != 0 && dto.CompanyId != null)
            {
                var company = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
                if (!company) return BadRequest("الشركة المطلوبة غير موجودة");
            }

            if (!string.IsNullOrEmpty(dto.ArtisanId) && dto.ArtisanId != "string")
            {
                var artisan = await _context.Artisans.AnyAsync(a => a.ApplicationUserId == dto.ArtisanId);
                if (!artisan) return BadRequest("الحرفي المطلوب غير موجود");
            }

            var newRequest = new ServiceRequest
            {
                FullName = dto.FullName,
                ServiceName = dto.ServiceName,
                Address = dto.Address,
                Governorate = dto.Governorate,
                ClientId = dto.ClientId,
                ArtisanId = (dto.ArtisanId == "string" || string.IsNullOrEmpty(dto.ArtisanId)) ? null : dto.ArtisanId,
                CompanyId = dto.CompanyId == 0 ? null : dto.CompanyId,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

            _context.ServiceRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            string targetId = newRequest.ArtisanId;
            if (string.IsNullOrEmpty(targetId) && newRequest.CompanyId.HasValue)
            {
                targetId = await _context.Companies
                    .Where(c => c.Id == newRequest.CompanyId)
                    .Select(c => c.ApplicationUserId)
                    .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrEmpty(targetId))
            {
                await _notificationService.SendNotificationAsync(
                    targetId,
                    "طلب جديد!",
                    $"قام {dto.FullName} بطلب خدمة: {dto.ServiceName}",
                    "Order",
                    "/orders/details/" + newRequest.Id
                );
            }

            return Ok(new { message = "تم إرسال طلبك بنجاح", requestId = newRequest.Id });
        }

        [Authorize(Roles = "Company,Artisan")]
        [HttpGet("incoming-requests")]
        public async Task<IActionResult> GetIncomingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var requests = await _context.ServiceRequests
                .Where(r => r.ArtisanId == userId || r.Company.ApplicationUserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new {
                    r.Id,
                    r.ServiceName,
                    ClientName = r.FullName,
                    Location = $"{r.Governorate} - {r.Address}",
                    r.RequestDate,
                    r.Status,
                    StatusArabic = GetStatusArabic(r.Status)
                }).ToListAsync();

            return Ok(requests);
        }

        [HttpGet("service-request-details/{requestId}")]
        public async Task<ActionResult> GetServiceRequestDetails(int requestId)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Artisan).ThenInclude(a => a.User)
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Id == requestId);

            if (request == null) return NotFound(new { message = "الطلب غير موجود" });

            return Ok(new
            {
                Id = request.Id,
                ServiceName = request.ServiceName,
                Status = request.Status.ToString(),
                StatusArabic = GetStatusArabic(request.Status),
                StatusDescription = GetStatusMessage(request.Status),
                OrderDetails = new
                {
                    Date = request.RequestDate.ToString("yyyy/MM/dd"),
                    ClientName = request.FullName,
                    Location = $"{request.Governorate} - {request.Address}"
                },
                ArtisanInfo = request.ArtisanId == null ? null : new
                {
                    Name = request.Artisan?.User?.FullName,
                    //Phone = request.Artisan?.User?.PhoneNumber
                },
                CompanyInfo = request.CompanyId == null ? null : new
                {
                    Name = request.Company?.CompanyName,
                    //Phone = request.Company?.User?.PhoneNumber
                }
            });
        }

        [Authorize(Roles = "Company,Artisan")]
        [HttpPut("update-request-status/{requestId}")]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, [FromBody] RequestStatus newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var request = await _context.ServiceRequests
                .Include(r => r.Company)
                .Include(r => r.Client) 
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return NotFound("الطلب غير موجود");

            if (request.ArtisanId != userId && (request.Company == null || request.Company.ApplicationUserId != userId))
                return Forbid();

            
            request.Status = newStatus;
            await _context.SaveChangesAsync();

            var targetUserId = request.Client?.ApplicationUserId;

            if (!string.IsNullOrEmpty(targetUserId))
            {
                await _notificationService.SendNotificationAsync(
                    targetUserId,
                    "تحديث بخصوص طلبك",
                    $"تغيرت حالة طلبك لخدمة {request.ServiceName} إلى {GetStatusArabic(newStatus)}",
                    "OrderUpdate",
                    "/my-orders"
                );
            }

            return Ok(new { message = "تم تحديث حالة الطلب بنجاح" });
        }
        [Authorize(Roles = "Company,Artisan")]
        [HttpGet("incoming-requests-tracking")]
        public async Task<IActionResult> GetIncomingRequestsTracking(RequestStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.ServiceRequests
                .Where(r => r.ArtisanId == userId || r.Company.ApplicationUserId == userId);


            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new {
                    r.Id,
                    r.ServiceName,
                    ClientName = r.FullName,
                    Location = $"{r.Governorate} - {r.Address}",
                    r.RequestDate,
                    r.Status,
                    StatusArabic = GetStatusArabic(r.Status)
                })
                .ToListAsync();

            return Ok(requests);
        }
        [Authorize(Roles = "Client")]
        [HttpPost("cancel-request/{id}")]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.ServiceRequests.Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id);

            if (request == null || request.Client.ApplicationUserId != userId) return Forbid();

            if (request.Status == RequestStatus.Pending || request.Status == RequestStatus.Accepted)
            {
                request.Status = RequestStatus.Cancelled;
                await _context.SaveChangesAsync();
                return Ok(new { message = "تم إلغاء الطلب" });
            }
            return BadRequest("لا يمكن إلغاء الطلب حالياً");
        }
    
        [Authorize(Roles = "Client")]
        [HttpPost("reorder/{oldRequestId}")]
        public async Task<IActionResult> Reorder(int oldRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var old = await _context.ServiceRequests
                .Include(r => r.Client)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == oldRequestId && r.ClientId == userId);

            if (old == null) return NotFound("الطلب الأصلي غير موجود");

            var newReq = new ServiceRequest
            {
                ServiceName = old.ServiceName,
                FullName = old.FullName,
                Address = old.Address,
                Governorate = old.Governorate,
                ClientId = old.ClientId,
                ArtisanId = old.ArtisanId,
                CompanyId = old.CompanyId,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

            _context.ServiceRequests.Add(newReq);
            await _context.SaveChangesAsync();

            string targetProviderId = old.ArtisanId;
            if (string.IsNullOrEmpty(targetProviderId) && old.CompanyId.HasValue)
            {
                targetProviderId = await _context.Companies
                    .Where(c => c.Id == old.CompanyId)
                    .Select(c => c.ApplicationUserId)
                    .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrEmpty(targetProviderId))
            {
                var clientName = old.Client?.User?.FullName ?? old.FullName;

                await _notificationService.SendNotificationAsync(
                    targetProviderId,
                    "إعادة طلب خدمة",
                    $"قام {clientName} بإعادة طلب خدمة: {old.ServiceName}",
                    "Reorder",
                    "/orders/details/" + newReq.Id
                );
            }

            return Ok(new { message = "تم إعادة إرسال الطلب بنجاح", requestId = newReq.Id });
        }

        [Authorize(Roles = "Company,Artisan")]
        [HttpPost("create-invoice/{requestId}")]
        public async Task<IActionResult> CreateInvoice(int requestId, [FromBody] List<CompanyInvoiceItemDto> items)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.ServiceRequests.FindAsync(requestId);

            if (request == null) return NotFound("الطلب غير موجود");

            if (request.ArtisanId != userId && (request.CompanyId == null || !_context.Companies.Any(c => c.Id == request.CompanyId && c.ApplicationUserId == userId)))
                return Forbid();

            var client = await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == request.ClientId);

            if (client == null)
            {
                client = await _context.Clients
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id.ToString() == request.ClientId);
            }

            if (client == null) return BadRequest("عذراً، لم نتمكن من الوصول لبيانات العميل المرتبطة بهذا الطلب");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    ApplicationUserId = client.ApplicationUserId,
                    OrderDate = DateTime.Now,
                    ArtisanId = request.ArtisanId,
                    CompanyId = request.CompanyId,
                    IsServiceOrder = true,
                    ServiceRequestId = requestId,
                    TotalPrice = 0
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var item in items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        CustomItemName = item.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ProductId = null
                    };

                    _context.OrderItems.Add(orderItem);
                    total += (item.UnitPrice * item.Quantity);
                }

                order.TotalPrice = total;
                request.Status = RequestStatus.Finished;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.SendNotificationAsync(
                    client.ApplicationUserId,
                    "تم إصدار فاتورة",
                    $"تم إنهاء الخدمة وإصدار فاتورة بقيمة {total} ج.م",
                    "Invoice",
                    "/invoice-details/" + order.Id
                );

                return Ok(new { message = "تم إصدار الفاتورة بنجاح", orderId = order.Id, total });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "خطأ في إنشاء الفاتورة");
            }
        }
        [HttpGet("request-details-for-provider/{requestId}")]
        public async Task<ActionResult> GetRequestDetailsForProvider(int requestId)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(s => s.Id == requestId);

            if (request == null) return NotFound(new { message = "الطلب غير موجود" });

            return Ok(new
            {
                Id = request.Id,
                ServiceName = request.ServiceName,
                Status = request.Status.ToString(),
                StatusArabic = GetStatusArabic(request.Status),
                OrderDetails = new
                {
                    Date = request.RequestDate.ToString("yyyy/MM/dd"),
                    ClientName = request.FullName,
                    Location = $"{request.Governorate} - {request.Address}",
                    PriceStatus = "لم يتم التحديد"
                }
            });
        }
        [HttpGet("invoice-details/{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {

            var order = await _context.Orders
                .Include(o => o.User) 
                .Include(o => o.Artisan).ThenInclude(a => a.User) 
                .Include(o => o.Company).ThenInclude(c => c.User) 
                .Include(o => o.OrderItems) 
                .Include(o => o.ServiceRequest) 
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "الفاتورة غير موجودة" });

            return Ok(new
            {
                InvoiceHeader = new
                {
                    InvoiceNumber = order.Id.ToString("D3"), 
                    Date = order.OrderDate.ToString("yyyy/MM/dd"),
                    ServiceType = order.ServiceRequest?.ServiceName ?? "خدمة فنية"
                },

                ClientInfo = new
                {
                    Name = order.ServiceRequest?.FullName ?? order.User?.FullName,
                    Address = order.ServiceRequest?.Address ?? order.User?.Governorate,
                },

                ProviderInfo = order.CompanyId != null ? new
                {
                    Type = "شركة",
                    Name = order.Company?.CompanyName,
                }
                : new
                {
                    Type = "حرفي",
                    Name = order.Artisan?.User?.FullName,
                },

                InvoiceItems = order.OrderItems?.Select(item => new
                {
                    ServiceName = item.CustomItemName ?? "خدمة صيانة",
                    Quantity = item.Quantity,
                    Price = item.UnitPrice,
                    Total = item.Quantity * item.UnitPrice
                }).ToList(),

                Summary = new
                {
                    FinalTotal = order.TotalPrice,
                    PaymentMethod = order.PaymentMethod.ToString() 
                }
            });
        }

        private static string GetStatusArabic(RequestStatus status) => status switch
        {
            RequestStatus.Pending => "في الانتظار",
            RequestStatus.Accepted => "تم القبول",
            RequestStatus.OnTheWay => "في الطريق",
            RequestStatus.Finished => "مكتمل",
            RequestStatus.Cancelled => "ملغي",
            _ => "غير معروف"
        };

        private static string GetStatusMessage(RequestStatus status) => status switch
        {
        RequestStatus.Pending => "تم إرسال الطلب وجاري انتظار الرد",
        RequestStatus.Accepted => "تم قبول طلبك",
            RequestStatus.Finished => "تم تنفيذ الخدمة بنجاح",
            RequestStatus.Cancelled => "تم إلغاء الطلب",
            _ => ""
        };
    }
}