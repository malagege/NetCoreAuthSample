using AuthSample.CustomMiddlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSample
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("AuthSampleDb"))
                );

            services.Configure<IdentityOptions>(options =>
            {
                // 密碼最小長度  (預設值:6)
                options.Password.RequiredLength = 6;
                // 密碼中允許最大重複字符數 (ex: "aaa123" ,"abbb123")  axabac 會過
                options.Password.RequiredUniqueChars = 3;
                // 至少使用非字母數字字符 (預設值: true)
                options.Password.RequireNonAlphanumeric = false;
                // 密碼是否包含小寫 (預設值: true)
                options.Password.RequireLowercase = false;
                // 密碼是否包含大寫 (預設值: true)
                options.Password.RequireUppercase = false;
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddEntityFrameworkStores<AppDbContext>();


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // 可參考，因為沒設定，暫時不使用
            // [[小菜一碟] 在 ASP.NET Core MVC 自訂 ExceptionHandler | 軟體主廚的程式料理廚房 - 點部落](https://www.dotblogs.com.tw/supershowwei/2021/04/14/185800)
            //else if (env.IsStaging() || env.IsProduction() || env.IsEnvironment("UAT"))
            //{
            //    app.UseExceptionHandler("/Error");
            //    app.UseStatusCodePagesWithReExecute("/Error/{0}");
            //}

            app.UseRouting();

            // 當帳號沒有檢查到登入資料，請確認這兩個是否加入 Middleware
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
