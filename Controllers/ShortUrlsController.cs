using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronBarCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Controllers
{
    public class ShortUrlsController : Controller
    {
        private const string _usableSymbols = "23456789bcdfghjkmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ-_";
        private static readonly int Base = _usableSymbols.Length;

        public static string Encode(int num)
        {
            var sb = new StringBuilder();
            while (num > 0)
            {
                sb.Insert(0, _usableSymbols.ElementAt(num % Base));
                num = num / Base;
            }
            return sb.ToString();
        }

        public static int Decode(string str)
        {
            var num = 0;
            for (var i = 0; i < str.Length; i++)
            {
                num = num * Base + _usableSymbols.IndexOf(str.ElementAt(i));
            }
            return num;
        }
        private readonly IShortUrlService _service;

        public ShortUrlsController(IShortUrlService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return RedirectToAction(actionName: nameof(Create));
        }

        public IActionResult Create()
        {
            ViewData["QRExample"] = BarcodeWriter.CreateBarcode(this.Url.ToString(), BarcodeEncoding.EAN13).ToDataUrl();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string originalUrl)
        {
            var shortUrl = new ShortUrl
            {
                OriginalUrl = originalUrl
            };

            TryValidateModel(shortUrl);
            if (ModelState.IsValid)
            {
                _service.Save(shortUrl);

                return RedirectToAction(actionName: nameof(Show), routeValues: new { id = shortUrl.Id });
            }

            return View(shortUrl);
        }

        public IActionResult Show(int? id)
        {
            if (!id.HasValue) 
            {
                return NotFound();
            }

            var shortUrl = _service.GetById(id.Value);
            if (shortUrl == null) 
            {
                return NotFound();
            }
            string showingString= Encode(shortUrl.Id);
            ViewData["Path"] = showingString;
            ViewData ["Image"] = BarcodeWriter.CreateBarcode(showingString, BarcodeEncoding.EAN13).ToDataUrl();

            return View(shortUrl);
        }

        [HttpGet("/ShortUrls/RedirectTo/{path:required}", Name = "ShortUrls_RedirectTo")]
        public IActionResult RedirectTo(string path)
        {
            if (path == null) 
            {
                return NotFound();
            }

            var shortUrl = _service.GetByPath(path);
            if (shortUrl == null) 
            {
                return NotFound();
            }

            return Redirect(shortUrl.OriginalUrl);
        }
    }
}
