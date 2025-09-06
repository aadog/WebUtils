using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;

namespace WebUtils
{
    public class RazorPageScript
    {
        public bool HasJs { get; set; } = false;
        public string ScriptPath { get; set; } = "";

        public RazorPageScript(ViewContext viewContext, IWebHostEnvironment environment, IMemoryCache cache,string viewPath = "/Pages", string scriptExt = ".js") 
        {
            var area = viewContext.RouteData.Values["area"]?.ToString();
            var page = viewContext.RouteData.Values["page"]?.ToString();
            string? jsRelativePath = null;
            if (!string.IsNullOrEmpty(page))
            {
                // 组合路径
                if (!string.IsNullOrEmpty(area))
                {
                    jsRelativePath = $"/Areas/{area}/Pages{page}.cshtml{scriptExt}";
                }
                else
                {
                    jsRelativePath = $"{viewPath}{page}.cshtml{scriptExt}";
                }
            }


            bool jsExists = false;
            if (!string.IsNullOrEmpty(jsRelativePath))
            {
                var physicalBasePath = environment.IsDevelopment() ? environment.ContentRootPath : environment.WebRootPath;

                var physicalPath = System.IO.Path.Combine(
                    physicalBasePath,
                    jsRelativePath.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar)
                );

                var cacheKey = "js_exists_" + physicalPath;
                jsExists = cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return System.IO.File.Exists(physicalPath);
                });
            }

            HasJs = jsExists;
            ScriptPath = jsRelativePath;
        }

        public string Render(bool aspAppendVersion=false)
        {
            if (HasJs != true || ScriptPath =="")
            {
                return "";
            }

            var appendStr = "asp-append-version=\"true\"";
            return $$$"""<script src="{{{ScriptPath}}}" {{{(aspAppendVersion ? appendStr : "")}}}></script>""";
        }
        public string RenderMosdule(bool isViteSrc=false,bool aspAppendVersion = false)
        {
            if (HasJs != true ||ScriptPath == "")
            {
                return "";
            }
            var appendStr = aspAppendVersion?"asp-append-version=\"true\"":"";
            var viteSrc = isViteSrc ? "vite-src" : "src";
            return $$$"""<script type="module"  {{{viteSrc}}}="{{{ScriptPath}}}" {{{appendStr}}}></script>""";
        }
    }
}
