using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Data;
using QualityControlSystem.Models;

namespace QualityControlSystem.Controllers
{
    public class FrameController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public FrameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LIST
        // =========================
        public async Task<IActionResult> Index()
        {
            var frames = await _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark)
                .Include(f => f.ProdProcessedSensors)
                .ToListAsync();

            return View(frames);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var frame = await _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark)
                .Include(f => f.Notifications)
                    .ThenInclude(n => n.ProdProcessedSensor)
                        .ThenInclude(p => p.Sensor)
                .Include(f => f.Notifications)
                    .ThenInclude(n => n.NotificationRule)
                .Include(f => f.ProdProcessedSensors)  // ← НОВОЕ: подгружаем обработанные датчики
                    .ThenInclude(p => p.Sensor)        // ← и название сенсора
                .FirstOrDefaultAsync(m => m.FrameId == id);

            if (frame == null)
                return NotFound();

            // JSON для визуальной оценки (оставляем как было)
            if (frame.VisualAnalysParams != null)
            {
                ViewBag.VisualJson = JsonSerializer.Serialize(
                    frame.VisualAnalysParams.RootElement,
                    new JsonSerializerOptions { WriteIndented = true }
                );
            }
            else
            {
                ViewBag.VisualJson = "{}";
            }

            return View(frame);
        }

        // =========================
        // CREATE
        // =========================
        public IActionResult Create()
        {
            ViewBag.ProdBatchId = new SelectList(
                _context.ProductionBatches.OrderByDescending(b => b.ProductionBatchId),
                "ProductionBatchId",
                "ProductionBatchId"
            );

            ViewBag.FrameModelId = new SelectList(_context.FrameModels, "FrameModelId", "FrameName");
            ViewBag.SystemMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Frame frame)
        {
            if (ModelState.IsValid)
            {
                _context.Add(frame);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProdBatchId = new SelectList(_context.ProductionBatches, "ProductionBatchId", "ProductionBatchId");
            ViewBag.FrameModelId = new SelectList(_context.FrameModels, "FrameModelId", "FrameName");
            ViewBag.SystemMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");

            return View(frame);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var frame = await _context.Frames.FindAsync(id);
            if (frame == null)
                return NotFound();

            ViewBag.ProdBatchId = new SelectList(
                _context.ProductionBatches.OrderByDescending(b => b.ProductionBatchId),
                "ProductionBatchId",
                "ProductionBatchId"
            );

            ViewBag.FrameModelId = new SelectList(_context.FrameModels, "FrameModelId", "FrameName");
            ViewBag.SystemMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");
            ViewBag.FinalMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");

            return View(frame);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Frame frame, string expertName, string expertResult)
        {
            if (id != frame.FrameId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var visualChecks = new List<object>();

                    foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("Status_")))
                    {
                        var param = key.Replace("Status_", "");

                        visualChecks.Add(new
                        {
                            param = param,
                            status = Request.Form[key],
                            comment = Request.Form[$"Comment_{param}"]
                        });
                    }

                    var jsonObject = new
                    {
                        expert = new
                        {
                            name = expertName,
                            date = DateTime.Now
                        },
                        visual_checks = visualChecks,
                        expert_result = expertResult
                    };

                    frame.VisualAnalysParams = JsonDocument.Parse(JsonSerializer.Serialize(jsonObject));

                    frame.ExpertMarkId = expertResult switch
                    {
                        "ГОДЕН" or "OK" => 1,
                        "НА ДОРАБОТКУ" or "REWORK" => 2,
                        "БРАК" or "REJECT" => 3,
                        _ => null
                    };

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

            ViewBag.ProdBatchId = new SelectList(_context.ProductionBatches, "ProductionBatchId", "ProductionBatchId");
            ViewBag.FrameModelId = new SelectList(_context.FrameModels, "FrameModelId", "FrameName");
            ViewBag.SystemMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");
            ViewBag.FinalMarkId = new SelectList(_context.FinalMarkTypes, "FinalMarkTypeId", "FinalMarkName");

            return View(frame);
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var frame = await _context.Frames
                .Include(f => f.Batch)
                .Include(f => f.FrameModel)
                .Include(f => f.SystemMark)
                .Include(f => f.ExpertMark)
                .Include(f => f.FinalMark)
                .FirstOrDefaultAsync(m => m.FrameId == id);

            if (frame == null)
                return NotFound();

            if (frame.VisualAnalysParams != null)
            {
                ViewBag.VisualJson = JsonSerializer.Serialize(
                    frame.VisualAnalysParams.RootElement,
                    new JsonSerializerOptions { WriteIndented = true }
                );
            }
            else
            {
                ViewBag.VisualJson = "{}";
            }

            return View(frame);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frame = await _context.Frames
                .Include(f => f.ProdProcessedSensors)
                .Include(f => f.Notifications)
                .FirstOrDefaultAsync(f => f.FrameId == id);

            if (frame != null)
            {
                _context.Frames.Remove(frame);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // VISUAL ANALYSIS
        // =====================================================
        public async Task<IActionResult> VisualAnalysis(int id)
        {
            var frame = await _context.Frames.FindAsync(id);
            if (frame == null)
                return NotFound();

            return View(frame);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVisualAnalysis(int frameId, string expertName, string expertResult)
        {
            var visualChecks = new List<object>();

            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("Status_")))
            {
                var param = key.Replace("Status_", "");
                visualChecks.Add(new
                {
                    param = param,
                    status = Request.Form[key],
                    comment = Request.Form[$"Comment_{param}"]
                });
            }

            var jsonObject = new
            {
                expert = new { name = expertName, date = DateTime.Now },
                visual_checks = visualChecks,
                expert_result = expertResult
            };

            var frame = await _context.Frames.FindAsync(frameId);
            if (frame == null)
                return NotFound();

            frame.VisualAnalysParams = JsonDocument.Parse(JsonSerializer.Serialize(jsonObject));

            frame.ExpertMarkId = expertResult switch
            {
                "ГОДЕН" or "OK" => 1,
                "НА ДОРАБОТКУ" or "REWORK" => 2,
                "БРАК" or "REJECT" => 3,
                _ => null
            };

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = frameId });
        }

        // =====================================================
        // ОБРАБОТКА ДАТЧИКОВ (ИСПРАВЛЕНО)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSensors(int frameId)
        {
            var frame = await _context.Frames
                .Include(f => f.ProdProcessedSensors)
                .FirstOrDefaultAsync(f => f.FrameId == frameId);

            if (frame == null)
                return NotFound();

            if (frame.ProdProcessedSensors.Any())
            {
                TempData["Info"] = "Датчики для этого каркаса уже были обработаны.";
                return RedirectToAction(nameof(Index));
            }

            var sensors = await _context.Sensors.ToListAsync();
            if (!sensors.Any())
            {
                TempData["Alert"] = "В системе не зарегистрировано ни одного датчика.";
                return RedirectToAction(nameof(Index));
            }

            // Группируем правила по SensorId: один сенсор → список правил
            var notificationRulesDict = await _context.NotificationRules
                .Include(nr => nr.NotificationType)
                .Where(nr => nr.SensorId != null)
                .GroupBy(nr => nr.SensorId.Value)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.ToList()
                );

            var processedRecords = new List<ProdProcessedSensor>();
            var notificationsToAdd = new List<Notification>();

            foreach (var sensor in sensors)
            {
                // Пропускаем сенсор с нулевым ID (защита)
                if (sensor.SensorId <= 0) continue;

                // Генерация тестового значения (в реальности — данные с оборудования)
                float value = _random.Next(2000, 5001);

                var record = new ProdProcessedSensor
                {
                    SensorId = sensor.SensorId,
                    FrameId = frame.FrameId,
                    ValueAfterProc = value,
                    ProcessTime = DateTime.Now
                };

                processedRecords.Add(record);

                // Проверяем все правила для данного сенсора
                if (notificationRulesDict.TryGetValue(sensor.SensorId, out var rules))
                {
                    foreach (var rule in rules)
                    {
                        bool exceedsNormal = rule.NormalValue.HasValue && value > rule.NormalValue.Value;
                        bool exceedsCritical = rule.CriticalValue.HasValue && value > rule.CriticalValue.Value;

                        if (exceedsNormal || exceedsCritical)
                        {
                            notificationsToAdd.Add(new Notification
                            {
                                NotificationRuleId = rule.NotificationRuleId,
                                FrameId = frame.FrameId,
                                NotificationTime = DateTime.Now
                                // SensorProdId заполним после сохранения processedRecords
                            });
                        }
                    }
                }
            }

            // Сохраняем обработанные значения датчиков
            _context.ProdProcessedSensors.AddRange(processedRecords);
            await _context.SaveChangesAsync();

            // Теперь, когда у processedRecords есть ID, привязываем их к уведомлениям
            if (notificationsToAdd.Any())
            {
                foreach (var notification in notificationsToAdd)
                {
                    var rule = await _context.NotificationRules
                        .FirstOrDefaultAsync(nr => nr.NotificationRuleId == notification.NotificationRuleId);

                    if (rule?.SensorId != null)
                    {
                        var processed = processedRecords
                            .FirstOrDefault(p => p.SensorId == rule.SensorId);

                        if (processed != null)
                        {
                            notification.SensorProdId = processed.ProdProcessedSensorId;
                        }
                    }
                }

                _context.Notifications.AddRange(notificationsToAdd);
                await _context.SaveChangesAsync();

                TempData["Alert"] = $"Обнаружены отклонения по {notificationsToAdd.Count} правил(ам) на датчиках.";
            }
            else
            {
                TempData["Success"] = "Датчики обработаны успешно. Отклонений не обнаружено.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS
        // =========================
        private bool FrameExists(int id)
        {
            return _context.Frames.Any(e => e.FrameId == id);
        }
    }
}