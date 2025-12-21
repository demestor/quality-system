using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Data;
using QualityControlSystem.Models;

namespace QualityControlSystem.Controllers
{
    public class ProductionBatchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionBatchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductionBatch
        public async Task<IActionResult> Index()
        {
            var batches = _context.ProductionBatches.Include(b => b.BatchStatus);
            return View(await batches.ToListAsync());
        }

        // GET: ProductionBatch/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var batch = await _context.ProductionBatches
                .Include(b => b.BatchStatus)
                .FirstOrDefaultAsync(m => m.ProductionBatchId == id);

            if (batch == null) return NotFound();

            return View(batch);
        }

        // GET: ProductionBatch/Create
        public IActionResult Create()
        {
            ViewData["BatchStatusId"] = new SelectList(_context.BatchStatuses, "BatchStatusId", "StatusName");
            return View();
        }

        // POST: ProductionBatch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartDate,EndDate,BatchStatusId,Recom")] ProductionBatch productionBatch)
        {
            if (ModelState.IsValid)
            {
                // Исправление ошибки с DateTime Kind для PostgreSQL
                if (productionBatch.StartDate.HasValue)
                    productionBatch.StartDate = DateTime.SpecifyKind(productionBatch.StartDate.Value, DateTimeKind.Utc);

                if (productionBatch.EndDate.HasValue)
                    productionBatch.EndDate = DateTime.SpecifyKind(productionBatch.EndDate.Value, DateTimeKind.Utc);

                _context.Add(productionBatch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BatchStatusId"] = new SelectList(_context.BatchStatuses, "BatchStatusId", "StatusName", productionBatch.BatchStatusId);
            return View(productionBatch);
        }

        // GET: ProductionBatch/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var productionBatch = await _context.ProductionBatches.FindAsync(id);
            if (productionBatch == null) return NotFound();

            ViewData["BatchStatusId"] = new SelectList(_context.BatchStatuses, "BatchStatusId", "StatusName", productionBatch.BatchStatusId);
            return View(productionBatch);
        }

        // POST: ProductionBatch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductionBatchId,StartDate,EndDate,BatchStatusId,Recom")] ProductionBatch productionBatch)
        {
            if (id != productionBatch.ProductionBatchId) return NotFound();

            if (ModelState.IsValid)
            {
                // Исправление DateTime Kind перед сохранением
                if (productionBatch.StartDate.HasValue)
                    productionBatch.StartDate = DateTime.SpecifyKind(productionBatch.StartDate.Value, DateTimeKind.Utc);

                if (productionBatch.EndDate.HasValue)
                    productionBatch.EndDate = DateTime.SpecifyKind(productionBatch.EndDate.Value, DateTimeKind.Utc);

                try
                {
                    _context.Update(productionBatch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductionBatchExists(productionBatch.ProductionBatchId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BatchStatusId"] = new SelectList(_context.BatchStatuses, "BatchStatusId", "StatusName", productionBatch.BatchStatusId);
            return View(productionBatch);
        }

        // GET: ProductionBatch/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var productionBatch = await _context.ProductionBatches
                .Include(b => b.BatchStatus)
                .FirstOrDefaultAsync(m => m.ProductionBatchId == id);

            if (productionBatch == null) return NotFound();

            return View(productionBatch);
        }

        // POST: ProductionBatch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productionBatch = await _context.ProductionBatches.FindAsync(id);
            if (productionBatch != null)
            {
                _context.ProductionBatches.Remove(productionBatch);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductionBatchExists(int id)
        {
            return _context.ProductionBatches.Any(e => e.ProductionBatchId == id);
        }
    }
}