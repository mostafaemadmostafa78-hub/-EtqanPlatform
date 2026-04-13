using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Client")]

    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public OrdersController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(OrderRequestDto dto)
        {
            var user = await _context.Users.AnyAsync(u => u.Id == dto.ApplicationUserId);
            if (!user) return BadRequest("User not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    ApplicationUserId = dto.ApplicationUserId,
                    OrderDate = DateTime.Now,
                    TotalPrice = 0,
                    OrderItems = new List<OrderItem>()
                };

                decimal finalPrice = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null) return NotFound($"Product {item.ProductId} not found");
                    if (product.StockQuantity < item.Quantity) return BadRequest($"No enough stock for {product.Name}");

                    var orderDetail = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Order = order
                    };

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
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error processing your order");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var response = new OrderResponseDto
            {
                OrderId = order.Id,
                Date = order.OrderDate,
                Total = order.TotalPrice,
                Details = order.OrderItems?.Select(x => new OrderItemResponseDto
                {
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity,
                    PriceAtPurchase = x.UnitPrice
                }).ToList() ?? new List<OrderItemResponseDto>()
            };

            return Ok(response);
        }
        //[HttpPost("checkout")]
        //[Authorize]
        //public async Task<IActionResult> Checkout([FromBody] List<CartItemDto> cartItems)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // 1. إنشاء الـ Order الأساسي
        //    var order = new Order
        //    {
        //        ApplicationUserId = userId,
        //        OrderDate = DateTime.Now,
        //        TotalPrice = cartItems.Sum(item => item.Price * item.Quantity)
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync(); // بنسيف عشان ناخد الـ OrderId

        //    // 2. تحويل كل حتة في السلة لـ OrderItem في الداتابيز
        //    foreach (var item in cartItems)
        //    {
        //        var orderItem = new OrderItem
        //        {
        //            OrderId = order.Id,
        //            ProductId = item.Id,
        //            Quantity = item.Quantity,
        //            UnitPrice = item.Price
        //        };
        //        _context.OrderItems.Add(orderItem);
        //    }

        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "تم إتمام الطلب بنجاح!" });
        //}

       // ah
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] List<CartItemDto> cartItems)
        {
            // 1. نجيب الـ UserId من التوكن عشان الأمان
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. استخدام Transaction عشان نضمن إن الطلب يتسيف كله أو لا شيء (كل المنتجات تتنقص من المخزن صح)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3. إنشاء الـ Order الأساسي
                var order = new Order
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    TotalPrice = 0 // هنحسبه في السيرفر أضمن
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // سيفنا عشان نحصل على OrderId

                decimal totalCalculatedPrice = 0;

                // 4. معالجة كل منتج في السلة
                foreach (var item in cartItems)
                {
                    // نأتي بالمنتج من الداتابيز (مهم جداً عشان السعر والمخزن)
                    var product = await _context.Products.FindAsync(item.Id);

                    if (product == null)
                        return BadRequest($"المنتج رقم {item.Id} غير موجود.");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"الكمية المطلوبة من {product.Name} غير متوفرة حالياً.");

                    // إنشاء بند الطلب
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price // نأخذ السعر من الداتابيز  
                    };

                    // 5. تحديث المخزن وحساب الإجمالي
                    product.StockQuantity -= item.Quantity;
                    totalCalculatedPrice += (product.Price * item.Quantity);

                    _context.OrderItems.Add(orderItem);
                }

                // 6. تحديث السعر النهائي في الطلب
                order.TotalPrice = totalCalculatedPrice;
                await _context.SaveChangesAsync();

                // تأكيد العملية كلها
                await transaction.CommitAsync();

                return Ok(new { message = "تم إتمام الطلب بنجاح!", orderId = order.Id, total = order.TotalPrice });
            }
            catch (Exception ex)
            {
                // لو حصل أي خطأ في النص، نلغي كل اللي حصل (Rollback)
                await transaction.RollbackAsync();
                return StatusCode(500, "حدث خطأ أثناء معالجة الطلب، حاول مرة أخرى.");
            }
        }
        //.

        //ah
        [HttpPost("reorder/{oldOrderId}")]
        public async Task<IActionResult> Reorder(int oldOrderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var oldOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == oldOrderId && o.ApplicationUserId == userId);

            if (oldOrder == null) return NotFound("الطلب غير موجود");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newOrder = new Order
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    TotalPrice = oldOrder.TotalPrice,
                    IsServiceOrder = oldOrder.IsServiceOrder,
                    ArtisanId = oldOrder.ArtisanId // لو كان طلب خدمة بننسخ الحرفي كمان
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // لو طلب منتجات (عِدد وأدوات) بننسخ الأصناف
                if (oldOrder.OrderItems != null && oldOrder.OrderItems.Any())
                {
                    foreach (var item in oldOrder.OrderItems)
                    {
                        _context.OrderItems.Add(new OrderItem
                        {
                            OrderId = newOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "تمت إعادة الطلب بنجاح" });
            }
            catch
            {
                await transaction.RollbackAsync();
                return BadRequest("فشل في تكرار الطلب");
            }
        }
        //.
        //ah

        [HttpPost("create-service-request")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
        {
            if (dto == null) return BadRequest("بيانات الطلب غير مكتملة");

            try
            {
                var newRequest = new ServiceRequest
                {
                    ServiceName = dto.ServiceName,
                    Description = dto.Description,
                    ArtisanId = dto.ArtisanId,
                    ClientId = dto.ClientId,
                    RequestDate = DateTime.Now,
                    Status = RequestStatus.Pending
                };

                _context.ServiceRequests.Add(newRequest);
                await _context.SaveChangesAsync(); // التأكد من الحفظ أولاً

                

                // 1. هنجيب اسم العميل عشان الحرفي يعرف مين باعتله
                var client = await _context.Users.FindAsync(dto.ClientId);
                var clientName = client?.FullName ?? "عميل جديد";

                // 2. نبعت الإشعار للحرفي (ArtisanId)
                await _notificationService.SendNotificationAsync(
                    dto.ArtisanId.ToString(),
                    "طلب جديد! ",
                    $"قام {clientName} بطلب خدمة: {dto.ServiceName}",
                    "/orders/details/" + newRequest.Id // رابط تفاصيل الطلب
                );

                // ------------------------------

                return Ok(new
                {
                    message = "تم إرسال طلبك بنجاح! يمكنك متابعته من شاشة طلباتك.",
                    requestId = newRequest.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "حدث خطأ أثناء إرسال الطلب، حاول مرة أخرى.");
            }
        }

        [HttpGet("invoice-details/{orderId}")]
        public async Task<IActionResult> GetDetailedInvoice(int orderId)
        {
            // 1.  البيانات من الداتابيز مع الربط بالجداول الأخرى 
            var order = await _context.Orders
                .Include(o => o.User)           // بيانات العميل
                .Include(o => o.Artisan)        // بيانات الحرفي
                    .ThenInclude(a => a.User)   // بيانات الحرفي الشخصية الاسم والموباي)
                .Include(o => o.OrderItems!)    // تفاصيل الجدول (الخدمات أو المعدات)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ServiceRequest) // الطلب الأصلي -لو موجود
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // 2. التحقق من وجود الفاتورة
            if (order == null)
            {
                return NotFound(new { message = "عذراً، الفاتورة غير موجودة" });
            }

            
            var invoiceData = new
            {
                // الجزء العلوي من التصميم
                Header = new
                {
                    InvoiceId = order.Id.ToString("D3"), // عشان تطلع 001
                    Date = order.OrderDate.ToString("yyyy/MM/dd"),
                    Title = order.IsServiceOrder ? "فاتورة خدمات" : "فاتورة شراء معدات"
                },

                // بيانات العميل اليمين
                Client = new
                {
                    Name = order.User?.FullName ?? "عميل تقني",
                    Phone = order.User?.PhoneNumber,
                    Address = order.User?.Governorate ?? "غير محدد",
                    Email = order.User?.Email
                },

                // بيانات الحرفي الشمال
                Artisan = order.IsServiceOrder ? new
                {
                    Name = order.Artisan?.User?.FullName,
                    Phone = order.Artisan?.User?.PhoneNumber,
                    Address = order.Artisan?.ServiceArea,
                    Email = order.Artisan?.User?.Email
                } : null,

                // جدول الخدمات/المعدات 
                Details = order.OrderItems?.Select(item => new {
                    ServiceName = item.Product?.Name ?? "خدمة صيانة",
                    Quantity = item.Quantity,
                    Price = item.UnitPrice,
                    Total = item.Quantity * item.UnitPrice
                }).ToList(),

                // ملخص الحسابات الأسفل
                Summary = new
                {
                    SubTotal = order.TotalPrice,
                    ServiceFees = order.IsServiceOrder ? 30 : 0, // مثال لرسوم الخدمة
                    Discount = 0,
                    FinalTotal = order.TotalPrice + (order.IsServiceOrder ? 30 : 0),
                    PaymentMethod = order.PaymentMethod == PaymentMethod.Cash ? "دفع كاش" : "بطاقة بنكية"
                }
            };

            return Ok(invoiceData);
        }
        //-----------------------------------------------------

        // 1. ميثود جلب تفاصيل طلب الخدمة 
        [HttpGet("service-request-details/{requestId}")]
        public async Task<ActionResult<ServiceRequestDetailsDto>> GetServiceRequestDetails(int requestId)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Artisan).ThenInclude(a => a.User)
                .Include(s => s.Client).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(s => s.Id == requestId);

            if (request == null)
                return NotFound(new { message = "هذا الطلب غير موجود" });

            // بناء الاستجابة بناءً على حالة الطلب 
            var response = new
            {
                Id = request.Id,
                ServiceName = request.ServiceName,
                Status = request.Status.ToString(), // القيمة الأصلية (Pending, Accepted, إلخ)
                StatusArabic = GetStatusArabic(request.Status), // الترجمة للعرض في التطبيق
                StatusDescription = GetStatusMessage(request.Status), // الرسالة التوضيحية اللي في الصورة

                OrderDetails = new
                {
                    Date = request.RequestDate.ToString("yyyy/MM/dd"),
                    Location = request.Client?.User?.Governorate ?? "غير محدد",
                    Description = request.Description ?? "لا يوجد وصف",
                    PriceStatus = request.Status == RequestStatus.Pending ? "لم يتم التحديد" : "تم الاتفاق"
                },

                // بيانات الحرفي تظهر فقط إذا تم قبول الطلب (Accepted وما بعدها)
                ArtisanInfo = (request.Status == RequestStatus.Pending || request.ArtisanId == null) ? null : new
                {
                    Name = request.Artisan?.User?.FullName,
                    Phone = request.Artisan?.User?.PhoneNumber,
                    Job = "فني متخصص"
                }
            };

            return Ok(response);
        }
        

        // 2. ميثود مساعدة لترجمة الحالات للعربية للعرض في الواجهة
        private string GetStatusArabic(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Pending => "في الانتظار",
                RequestStatus.Accepted => "تم القبول",
                RequestStatus.OnTheWay => "في الطريق",
                RequestStatus.Finished => "مكتمل",
                RequestStatus.Cancelled => "ملغي",
                _ => "غير معروف"
            };
        }

        // 3. ميثود مساعدة لجلب الرسالة التوضيحية (التي تظهر تحت اسم الخدمة في الصورة)
        private string GetStatusMessage(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Pending => "تم إرسال الطلب للحرفي وجاري انتظار الرد",
                RequestStatus.Accepted => "وافق الحرفي على طلبك، سيتم التواصل معك قريباً",
                RequestStatus.OnTheWay => "الحرفي الآن في طريقه إليك",
                RequestStatus.Finished => "تم تنفيذ الخدمة بنجاح، يمكنك تقييم الحرفي الآن",
                RequestStatus.Cancelled => "عذراً، تم إلغاء هذا الطلب",
                _ => ""
            };
        }

        // 4. ميثود إلغاء الطلب تحديث الحالة بدلاً من الحذف النهائي
        [HttpPost("cancel-request/{id}")]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);

            if (request == null) return NotFound();

            // العميل يقدر يلغي الطلب فقط لو لسه "Pending" أو "Accepted" (قبل ما الحرفي يتحرك)
            if (request.Status == RequestStatus.Pending || request.Status == RequestStatus.Accepted)
            {
                request.Status = RequestStatus.Cancelled; 
                await _context.SaveChangesAsync();
                return Ok(new { message = "تم إلغاء الطلب بنجاح" });
            }

            return BadRequest(new { message = "عذراً، لا يمكن إلغاء الطلب في هذه المرحلة" });
        }
        //.

        //[HttpPost]
        //public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var order = new Order
        //    {
        //        ApplicationUserId = userId,
        //        OrderDate = DateTime.Now,
        //        TotalPrice = 0
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    foreach (var item in dto.Items)
        //    {
        //        var product = await _context.Products.FindAsync(item.ProductId);

        //        if (product == null)
        //            return BadRequest("Product not found");

        //        if (product.StockQuantity < item.Quantity)
        //            return BadRequest("Not enough stock");

        //        var orderItem = new OrderItem
        //        {
        //            OrderId = order.Id,
        //            ProductId = product.Id,
        //            Quantity = item.Quantity,
        //            Price = product.Price
        //        };

        //        product.StockQuantity -= item.Quantity;
        //        order.TotalPrice += product.Price * item.Quantity;

        //        _context.OrderItems.Add(orderItem);
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(order);
        //}



    }
}

