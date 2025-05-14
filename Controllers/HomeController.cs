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
    /// ���������� ��� ���������� �������� ��������� � ���������� � ��������� �������.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ElasticSearchService _elasticSearchService;
        private readonly MongoDbService _mongoDbService;

        /// <summary>
        /// ����������� ����������� HomeController.
        /// </summary>
        /// <param name="elasticSearchService">������ ��� ������ � ElasticSearch.</param>
        /// <param name="mongoDbService">������ ��� ������ � MongoDB.</param>
        public HomeController(ElasticSearchService elasticSearchService, MongoDbService mongoDbService)
        {
            _elasticSearchService = elasticSearchService;
            _mongoDbService = mongoDbService;
        }

        /// <summary>
        /// ���������� ������� �������� ����������.
        /// </summary>
        /// <returns>������������� Index.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// �������������� ���� ������ MongoDB ������� �� test-data.json.
        /// </summary>
        /// <returns>������������� Index � ���������� �� �������������.</returns>
        public async Task<IActionResult> InitializeDatabase()
        {
            try
            {
                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json");

                if (!System.IO.File.Exists(dataPath))
                {
                    ViewBag.Message = "������: ���� � ������� �� ������";
                    return View("Index");
                }

                var json = await System.IO.File.ReadAllTextAsync(dataPath);
                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                await _mongoDbService.InitializeDatabaseAsync(products);

                ViewBag.Message = $"���� ����������������! ��������� {products?.Count ?? 0} �������.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"������: {ex.Message}";
            }

            return View("Index");
        }

        /// <summary>
        /// ��������� ����� ������� � ���� ������ MongoDB � ������ �������� � ���������.
        /// </summary>
        /// <param name="query">������ ������ �� ����� ��� �������� ������.</param>
        /// <param name="category">��������� ������ ��� ����������.</param>
        /// <param name="minPrice">����������� ���� ��� ����������.</param>
        /// <param name="maxPrice">������������ ���� ��� ����������.</param>
        /// <param name="tags">���� ��� ���������� (����� �������).</param>
        /// <param name="page">����� ������� ��������.</param>
        /// <param name="sort">���� ��� ���������� (�� ��������� "name").</param>
        /// <returns>������������� Index � ������������ ������.</returns>
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
        /// ���������� ������ JSON-����� ��� �������� ������.
        /// </summary>
        /// <returns>���� sample-data.json � �������� ������.</returns>
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
                        Name = "������ ������",
                        Description = "�������� �������",
                        Price = 100,
                        Category = "���������",
                        Tags = new System.Collections.Generic.List<string> { "���1", "���2" },
                        CreatedAt = DateTime.UtcNow
                    }
                };

                var json = JsonSerializer.Serialize(sampleData, new JsonSerializerOptions { WriteIndented = true });
                return File(Encoding.UTF8.GetBytes(json), "application/json", "sample-data.json");
            }

            return PhysicalFile(samplePath, "application/json", "sample-data.json");
        }

        /// <summary>
        /// ��������� JSON-���� � ������� ��� ������������� ���� ������.
        /// </summary>
        /// <param name="jsonFile">����������� JSON-����.</param>
        /// <returns>������������� Index � ���������� � ���������� ��������.</returns>
        [HttpPost]
        public async Task<IActionResult> UploadData(IFormFile jsonFile)
        {
            try
            {
                if (jsonFile == null || jsonFile.Length == 0)
                {
                    ViewBag.Message = "������: ���� �� ������";
                    return View("Index");
                }

                if (Path.GetExtension(jsonFile.FileName).ToLower() != ".json")
                {
                    ViewBag.Message = "������: ����� ��������� ������ JSON-�����";
                    return View("Index");
                }

                using var streamReader = new StreamReader(jsonFile.OpenReadStream());
                var jsonContent = await streamReader.ReadToEndAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var products = JsonSerializer.Deserialize<System.Collections.Generic.List<Product>>(jsonContent, options);

                if (products == null || !products.Any())
                {
                    ViewBag.Message = "������: ���� �� �������� �������� ������";
                    return View("Index");
                }

                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "test-data.json");
                await System.IO.File.WriteAllTextAsync(dataPath, jsonContent);

                ViewBag.Message = "���� ������� ��������! ������ �� ������ ���������������� ���� � ������ �������.";
            }
            catch (JsonException)
            {
                ViewBag.Message = "������: �������� ������ JSON-�����";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"������: {ex.Message}";
            }

            return View("Index");
        }
    }
}