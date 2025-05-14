using System.Diagnostics;
using CatalogueWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CatalogueWebApp.Services;
using Microsoft.AspNetCore.Http;

namespace CatalogueWebApp.Controllers
{
    /// <summary>
    /// Контроллер для управления домашней страницей и операциями с каталогом товаров.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ElasticSearchService _elasticSearchService;
        private readonly MongoDbService _mongoDbService;

        /// <summary>
        /// Конструктор контроллера HomeController.
        /// </summary>
        /// <param name="elasticSearchService">Сервис для работы с ElasticSearch.</param>
        /// <param name="mongoDbService">Сервис для работы с MongoDB.</param>
        public HomeController(ElasticSearchService elasticSearchService, MongoDbService mongoDbService)
        {
            _elasticSearchService = elasticSearchService;
            _mongoDbService = mongoDbService;
        }

        /// <summary>
        /// Возвращает главную страницу приложения.
        /// </summary>
        /// <returns>Представление Index.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Инициализирует базу данных MongoDB данными из test-data.json.
        /// </summary>
        /// <returns>Представление Index с сообщением об инициализации.</returns>
        public async Task<IActionResult> InitializeDatabase()
        {
            try
            {
                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json");

                if (!System.IO.File.Exists(dataPath))
                {
                    ViewBag.Message = "Ошибка: Файл с данными не найден";
                    return View("Index");
                }

                var json = await System.IO.File.ReadAllTextAsync(dataPath);
                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                await _mongoDbService.InitializeDatabaseAsync(products);

                ViewBag.Message = $"База инициализирована! Добавлено {products?.Count ?? 0} товаров.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ошибка: {ex.Message}";
            }

            return View("Index");
        }

        /// <summary>
        /// Выполняет поиск товаров в базе данных MongoDB с учетом фильтров и пагинации.
        /// </summary>
        /// <param name="query">Строка поиска по имени или описанию товара.</param>
        /// <param name="category">Категория товара для фильтрации.</param>
        /// <param name="minPrice">Минимальная цена для фильтрации.</param>
        /// <param name="maxPrice">Максимальная цена для фильтрации.</param>
        /// <param name="tags">Теги для фильтрации (через запятую).</param>
        /// <param name="page">Номер текущей страницы.</param>
        /// <param name="sort">Поле для сортировки (по умолчанию "name").</param>
        /// <returns>Представление Index с результатами поиска.</returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Search(string query, string category, decimal? minPrice, decimal? maxPrice, string tags, int page = 1, string sort = "name")
        {
            var products = await _mongoDbService.GetProductsAsync();

            if (!string.IsNullOrEmpty(query))
                products = products.Where(p => p.Name.Contains(query) || p.Description.Contains(query)).ToList();

            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category == category).ToList();

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();

            if (!string.IsNullOrEmpty(tags))
            {
                var tagList = tags.Split(',').Select(t => t.Trim()).ToList();
                products = products.Where(p => p.Tags.Intersect(tagList).Any()).ToList();
            }

            switch (sort)
            {
                case "name":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "category":
                    products = products.OrderBy(p => p.Category).ToList();
                    break;
                default:
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
            }

            int pageSize = 5;
            var totalPages = (int)Math.Ceiling(products.Count() / (double)pageSize);
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Query = query;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Tags = tags;
            ViewBag.Sort = sort;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View("Index", products);
        }

        /// <summary>
        /// Возвращает пример JSON-файла для загрузки данных.
        /// </summary>
        /// <returns>Файл sample-data.json с примером данных.</returns>
        [HttpGet]
        public IActionResult DownloadSample()
        {
            var samplePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json");

            if (!System.IO.File.Exists(samplePath))
            {
                var sampleData = new[]
                {
                    new Product
                    {
                        Name = "Пример товара",
                        Description = "Описание примера",
                        Price = 100,
                        Category = "Категория",
                        Tags = new System.Collections.Generic.List<string> { "тег1", "тег2" },
                        CreatedAt = DateTime.UtcNow
                    }
                };

                var json = JsonSerializer.Serialize(sampleData, new JsonSerializerOptions { WriteIndented = true });
                return File(Encoding.UTF8.GetBytes(json), "application/json", "sample-data.json");
            }

            return PhysicalFile(samplePath, "application/json", "sample-data.json");
        }

        /// <summary>
        /// Загружает JSON-файл с данными для инициализации базы данных.
        /// </summary>
        /// <param name="jsonFile">Загруженный JSON-файл.</param>
        /// <returns>Представление Index с сообщением о результате загрузки.</returns>
        [HttpPost]
        public async Task<IActionResult> UploadData(IFormFile jsonFile)
        {
            try
            {
                if (jsonFile == null || jsonFile.Length == 0)
                {
                    ViewBag.Message = "Ошибка: Файл не выбран";
                    return View("Index");
                }

                if (Path.GetExtension(jsonFile.FileName).ToLower() != ".json")
                {
                    ViewBag.Message = "Ошибка: Можно загружать только JSON-файлы";
                    return View("Index");
                }

                using var streamReader = new StreamReader(jsonFile.OpenReadStream());
                var jsonContent = await streamReader.ReadToEndAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var products = JsonSerializer.Deserialize<System.Collections.Generic.List<Product>>(jsonContent, options);

                if (products == null || !products.Any())
                {
                    ViewBag.Message = "Ошибка: Файл не содержит валидных данных";
                    return View("Index");
                }

                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json");
                await System.IO.File.WriteAllTextAsync(dataPath, jsonContent);

                ViewBag.Message = "Файл успешно загружен! Теперь вы можете инициализировать базу с новыми данными.";
            }
            catch (JsonException)
            {
                ViewBag.Message = "Ошибка: Неверный формат JSON-файла";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ошибка: {ex.Message}";
            }

            return View("Index");
        }
    }
}