using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Infoware.EntityFrameworkCore.MemoryCache.Extensions;
namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BlogContext _blogContext;

        public IndexModel(BlogContext blogContext)
        {
            _blogContext = blogContext;
        }

        public async void OnGet()
        {
            //var migrator = _blogContext.Database.GetService<IMigrator>();
            //await migrator.MigrateAsync();

            //_blogContext.Add(new Blog()
            //{
            //    Name = "Test",
            //});

            var query = _blogContext.Set<Blog>().CacheableTagWith("blogskey", new TimeSpan(0, 0, 20));
            var res1 = await EntityFrameworkQueryableExtensions.CountAsync(query.TagWith("[[COUNT]]"));
            var res2 = await EntityFrameworkQueryableExtensions.ToListAsync(query.Skip(2));
            int a = 1;
        }
    }
}