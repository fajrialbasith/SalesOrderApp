using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesOrderApp.Data;
using SalesOrderApp.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace SalesOrderApp.Controllers
{
    public class SalesOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword, DateTime? orderDate)
        {
            var salesOrders = from s in _context.SalesOrders
                              join c in _context.Customers on s.COM_CUSTOMER_ID equals c.COM_CUSTOMER_ID
                              select new SalesOrderViewModel
                              {
                                  SO_ORDER_ID = s.SO_ORDER_ID,
                                  ORDER_NO = s.ORDER_NO,
                                  ORDER_DATE = s.ORDER_DATE,
                                  COM_CUSTOMER_ID = s.COM_CUSTOMER_ID,
                                  ADDRESS = s.ADDRESS,
                                  CUSTOMER_NAME = c.CUSTOMER_NAME
                              };

            if (!string.IsNullOrEmpty(keyword))
            {
                salesOrders = salesOrders.Where(s => s.ORDER_NO.Contains(keyword) || s.CUSTOMER_NAME.Contains(keyword));
            }

            if (orderDate.HasValue)
            {
                salesOrders = salesOrders.Where(s => s.ORDER_DATE == orderDate.Value);
            }

            return View(await salesOrders.ToListAsync());
        }

        public IActionResult ExportToExcel()
        {
            var salesOrders = _context.SalesOrders
                                      .Include(s => s.Customer)
                                      .Select(s => new SalesOrderViewModel
                                      {
                                          SO_ORDER_ID = s.SO_ORDER_ID,
                                          ORDER_NO = s.ORDER_NO,
                                          ORDER_DATE = s.ORDER_DATE,
                                          COM_CUSTOMER_ID = s.COM_CUSTOMER_ID,
                                          ADDRESS = s.ADDRESS,
                                          CUSTOMER_NAME = s.Customer.CUSTOMER_NAME
                                      })
                                      .ToList();

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("SalesOrders");
                worksheet.Cells.LoadFromCollection(salesOrders, true);
                package.Save();
            }
            stream.Position = 0;
            string excelName = $"SalesOrders-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

            public IActionResult Create()
        {
            ViewBag.Customers = new SelectList(new[] { 
                new { COM_CUSTOMER_ID = 1, CUSTOMER_NAME = "TestCustomer1" }, 
                new { COM_CUSTOMER_ID = 2, CUSTOMER_NAME = "TestCustomer2" } 
            }, "COM_CUSTOMER_ID", "CUSTOMER_NAME");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SO_ORDER_ID,ORDER_NO,ORDER_DATE,COM_CUSTOMER_ID,ADDRESS")] SalesOrder salesOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salesOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(new[] { "TestCustomer1", "TestCustomer2" });
            return View(salesOrder);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder == null)
            {
                return NotFound();
            }
            return View(salesOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SO_ORDER_ID,ORDER_NO,ORDER_DATE,COM_CUSTOMER_ID,ADDRESS")] SalesOrder salesOrder)
        {
            if (id != salesOrder.SO_ORDER_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salesOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderExists(salesOrder.SO_ORDER_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(salesOrder);
        }

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.SO_ORDER_ID == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(m => m.SO_ORDER_ID == id);
            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder != null)
            {
                _context.SalesOrders.Remove(salesOrder);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
