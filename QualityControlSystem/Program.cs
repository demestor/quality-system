using Microsoft.EntityFrameworkCore;
using QualityControlSystem.Data;
using EFCore.NamingConventions; // оставляем, если используешь пакет

namespace QualityControlSystem
{
    public class Program
    {
        // Важно: async Task Main — чтобы можно было использовать await
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Настройка DbContext с Npgsql
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.SetPostgresVersion(15, 0);
                    })
                .UseSnakeCaseNamingConvention()     // CamelCase → snake_case
                .UseLowerCaseNamingConvention()     // Принудительно нижний регистр без кавычек
            );

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            var app = builder.Build();

            // === Диагностика базы данных при старте приложения ===
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    // 1. Проверка подключения
                    bool canConnect = await db.Database.CanConnectAsync();
                    app.Logger.LogInformation("Подключение к базе данных PostgreSQL: {CanConnect}", canConnect);
                    app.Logger.LogInformation("Текущая база данных: {DatabaseName}", db.Database.GetDbConnection().Database);

                    if (!canConnect)
                    {
                        app.Logger.LogError("Не удалось подключиться к базе данных. Проверьте строку подключения.");
                    }
                    else
                    {
                        // 2. Получаем реальный список таблиц из PostgreSQL
                        var connection = db.Database.GetDbConnection();
                        await connection.OpenAsync();

                        var command = connection.CreateCommand();
                        command.CommandText = @"
                            SELECT tablename
                            FROM pg_tables
                            WHERE schemaname = 'public'
                            ORDER BY tablename;";

                        var tables = new List<string>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(reader.GetString(0));
                            }
                        }

                        await connection.CloseAsync();

                        // 3. Выводим найденные таблицы
                        if (tables.Any())
                        {
                            app.Logger.LogInformation("Найдено таблиц в схеме public: {Count}", tables.Count);
                            foreach (var table in tables)
                            {
                                app.Logger.LogInformation("  → {TableName}", table);
                            }

                            // 4. Проверяем наличие нужной таблицы
                            if (tables.Contains("production_batch"))
                            {
                                app.Logger.LogInformation("Таблица 'production_batch' НАЙДЕНА в PostgreSQL!");

                                // 5. Пробуем запрос через EF Core
                                try
                                {
                                    int count = await db.ProductionBatches.CountAsync();
                                    app.Logger.LogInformation("Через EF Core прочитано записей из production_batch: {Count}", count);
                                }
                                catch (Exception efEx)
                                {
                                    app.Logger.LogError(efEx, "EF Core НЕ МОЖЕТ обратиться к production_batch, хотя таблица существует!");
                                }
                            }
                            else
                            {
                                app.Logger.LogWarning("Таблица 'production_batch' ОТСУТСТВУЕТ в списке таблиц PostgreSQL!");
                            }
                        }
                        else
                        {
                            app.Logger.LogWarning("В схеме public НЕТ НИ ОДНОЙ таблицы! База пустая или подключение к другой базе.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Критическая ошибка при работе с базой данных на старте приложения");
                }
            }
            // === Конец диагностики ===

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}