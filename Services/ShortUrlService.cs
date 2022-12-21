using UrlShortener.Controllers;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly UrlShortenerContext _context;

        public ShortUrlService(UrlShortenerContext context)
        {
            _context = context;
        }

        public ShortUrl GetById(int id)
        {
            return _context.ShortUrls.Find(id);
        }

        public ShortUrl GetByPath(string path)
        {
            return _context.ShortUrls.Find(ShortUrlsController.Decode((path)));
        }

        public ShortUrl GetByOriginalUrl(string originalUrl)
        {
            var t = _context.ShortUrls;
            
            foreach (var shortUrl in _context.ShortUrls) {
                if (shortUrl.OriginalUrl == originalUrl) {
                    return shortUrl;
                }
            }

            return null;
        }

        public int Save(ShortUrl shortUrl)
        {
            _context.ShortUrls.Add(shortUrl);
            _context.SaveChanges();

            return shortUrl.Id;
        }
    }
}
