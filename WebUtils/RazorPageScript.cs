using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebUtils
{
    public class RazorPageScript
    {
        public bool HasJs { get; set; } = false;
        public string ScriptPath { get; set; } = "";

        public string WebScriptPath { get; set; } = "";
        private ViewContext _viewContext;
        private IWebHostEnvironment _environment;
        private IMemoryCache _cache;
        private string _scriptExt=".js";
        private string _viewPath = "/Pages";
        private string _areaViewPath = "/Pages";
        public RazorPageScript(ViewContext viewContext, IWebHostEnvironment environment, IMemoryCache cache) 
        {
           _viewContext = viewContext;
           _environment = environment;
           _cache = cache;
        }

        public RazorPageScript SetScriptExt(string ext)
        {
            _scriptExt = ext;
            return this;
        }

        public RazorPageScript SetViewPath(string path)
        {
            _viewPath = path;
            return this;
        }

        public RazorPageScript SetAreaViewPath(string path)
        {
            _areaViewPath = path;
            return this;
        }
        public bool Check()
        { 
            var area = _viewContext.RouteData.Values["area"]?.ToString();
            var page = _viewContext.RouteData.Values["page"]?.ToString();
            string? jsRelativePath = null;
            if (!string.IsNullOrEmpty(page))
            {
                // 组合路径
                if (!string.IsNullOrEmpty(area))
                {
                    jsRelativePath = $"/Areas/{area}{_areaViewPath}{page}.cshtml{_scriptExt}";
                }
                else
                {
                    jsRelativePath = $"{_viewPath}{page}.cshtml{_scriptExt}";
                }
            }


            bool jsExists = false;
            if (!string.IsNullOrEmpty(jsRelativePath))
            {
                var physicalBasePath = _environment.IsDevelopment() ? _environment.ContentRootPath : _environment.WebRootPath;

                var physicalPath = Path.Combine(
                    physicalBasePath,
                    jsRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                var cacheKey = "js_exists_" + physicalPath;
                jsExists = _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return File.Exists(physicalPath);
                });
            }

            HasJs = jsExists;
            ScriptPath = jsRelativePath;
            WebScriptPath = $"~{ScriptPath}";
            return HasJs == true && ScriptPath != "";
        }

        public static RazorPageScript Create(ViewContext viewContext, IWebHostEnvironment environment, IMemoryCache cache)
        {
            var p = new RazorPageScript(viewContext, environment, cache);
            return p;
        }
    }
}
