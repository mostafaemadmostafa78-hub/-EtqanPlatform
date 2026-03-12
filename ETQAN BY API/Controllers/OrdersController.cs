using ETQAN.API.Data;
using ETQAN.API.Models;
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
  //  [Authorize(Roles = "Client")]

    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
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
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var response = new OrderResponseDto
            {
                OrderId = order.Id,
                Date = order.OrderDate,
                Total = order.TotalPrice,
                Details = order.OrderItems.Select(x => new OrderItemResponseDto
                {
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity,
                    PriceAtPurchase = x.UnitPrice
                }).ToList()
            };

            return Ok(response);
        }
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

