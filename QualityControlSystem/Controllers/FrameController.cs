// FrameController.cs
// Разместить в папке Controllers

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Data;
using QualityControlSystem.Models;
using System.Text.Json;

namespace QualityControlSystem.Controllers
{
    public class FrameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FrameController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateSelectLists(Frame? model = null)
        {
            ViewData["ProdBatchId"] = new SelectList(_context.ProductionBatches, "ProductionBatchId", "StartDate", model?.ProdBatchId);
            ViewData["FrameModelId"] = new SelectList(_context.FrameModels, "FrameModelId", "FrameName", model?.FrameModelId);
            ViewData["SystemMarkId"] = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName", model?.SystemMarkId);
            ViewData["ExpertMarkId"] = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName", model?.ExpertMarkId);
            ViewData["FinalMarkId"] = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName", model?.FinalMarkId);
        }

        // GET: Frame
        public async Task<IActionResult> Index()
        {
            var frames = _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark);
            return View(await frames.ToListAsync());
        }

        // GET: Frame/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var frame = await _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark)
                .FirstOrDefaultAsync(m => m.FrameId == id);
            if (frame == null) return NotFound();

            // Для отображения JSON в Details
            ViewBag.VisualJson = frame.VisualAnalysParams?.RootElement.GetRawText() ?? "{}";
            return View(frame);
        }

        // GET: Frame/Create
        public IActionResult Create()
        {
            PopulateSelectLists();
            return View(new Frame { VisualJson = "{}" });
        }

        // POST: Frame/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Frame frame)
        {
            // Убираем VisualJson из валидации модели, чтобы не ругался на [NotMapped]
            ModelState.Remove(nameof(Frame.VisualJson));

            if (ModelState.IsValid)
            {
                // Обработка JSON из textarea
                if (!string.IsNullOrWhiteSpace(frame.VisualJson))
                {
                    try
                    {
                        frame.VisualAnalysParams = JsonDocument.Parse(frame.VisualJson.Trim());
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError(nameof(Frame.VisualJson), "Некорректный формат JSON. Проверьте синтаксис.");
                        PopulateSelectLists(frame);
                        return View(frame);
                    }
                }
                else
                {
                    frame.VisualAnalysParams = null; // если поле пустое
                }

                _context.Add(frame);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Если валидация не прошла (например, обязательные поля)
            PopulateSelectLists(frame);
            return View(frame);
        }

        // GET: Frame/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var frame = await _context.Frames
                .AsNoTracking() 
                .FirstOrDefaultAsync(f => f.FrameId == id);

            if (frame == null) return NotFound();

            // Заполняем временное поле для отображения в textarea
            frame.VisualJson = frame.VisualAnalysParams?.RootElement.GetRawText() ?? "{}";

            PopulateSelectLists(frame);
            return View(frame);
        }

        // POST: Frame/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Frame frame)
        {
            if (id != frame.FrameId) return NotFound();

            ModelState.Remove(nameof(Frame.VisualJson));

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(frame.VisualJson))
                {
                    try
                    {
                        frame.VisualAnalysParams = JsonDocument.Parse(frame.VisualJson.Trim());
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError(nameof(Frame.VisualJson), "Некорректный формат JSON. Проверьте синтаксис.");
                        PopulateSelectLists(frame);
                        return View(frame);
                    }
                }
                else
                {
                    frame.VisualAnalysParams = null;
                }

                try
                {
                    _context.Update(frame);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FrameExists(frame.FrameId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateSelectLists(frame);
            return View(frame);
        }

        // GET: Frame/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var frame = await _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark)
                .FirstOrDefaultAsync(m => m.FrameId == id);
            if (frame == null) return NotFound();

            ViewBag.VisualJson = frame.VisualAnalysParams?.RootElement.GetRawText() ?? "{}";
            return View(frame);
        }

        // POST: Frame/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frame = await _context.Frames.FindAsync(id);
            if (frame != null)
            {
                _context.Frames.Remove(frame);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FrameExists(int id)
        {
            return _context.Frames.Any(e => e.FrameId == id);
        }
    }
}