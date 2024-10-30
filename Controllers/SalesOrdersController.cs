using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                              select new { s, c.CUSTOMER_NAME };

            if (!string.IsNullOrEmpty(keyword))
            {
                salesOrders = salesOrders.Where(s => s.s.ORDER_NO.Contains(keyword) || s.CUSTOMER_NAME.Contains(keyword));
            }

            if (orderDate.HasValue)
            {
                salesOrders = salesOrders.Where(s => s.s.ORDER_DATE == orderDate.Value);
            }

            return View(await salesOrders.ToListAsync());
        }

        public IActionResult ExportToExcel()
        {
            var salesOrders = _context.SalesOrders.ToList();
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
            ViewBag.Customers = _context.Customers.ToList();
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
            ViewBag.Customers = _context.Customers.ToList();
            return View(salesOrder);
        }


        // GET: SalesOrders/Edit/5
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

        // POST: SalesOrders/Edit/5
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

        // GET: SalesOrders/Delete/5
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

        // POST: SalesOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);
            _context.SalesOrders.Remove(salesOrder);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}