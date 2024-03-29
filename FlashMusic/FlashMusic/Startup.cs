using FlashMusic.Database;
using FlashMusic.Models;
using FlashMusic.Services;
using FlashMusic.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashMusic
{
    public class Startup
    {

        private readonly string AllowCors = "AllowCors";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // 在此处添加服务
        public void ConfigureServices(IServiceCollection services)
        {
            // JWT的使用
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)      // 默认授权机制名称
                .AddJwtBearer(options =>
                {
                    // options.RequireHttpsMetadata = false;
                    // options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,          // 是否在令牌期间验证签发者
                        ValidateAudience = true,        // 是否验证签收者
                        ValidateLifetime = true,        // 是否验证失效时间
                        ValidateIssuerSigningKey = true,    // 是否验证签名
                        ValidIssuer = Configuration["Jwt:Issuer"],      // 签发者，签发Token的人
                        ValidAudience = Configuration["Jwt:Audience"],  // 接收者
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),    // 拿到加密后的key
                        ClockSkew = TimeSpan.Zero
                    };
                });


            // 将Controller的核心服务注册到容器中去
            services.AddControllers(setup =>
            {
                setup.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson(setup =>
            {
                setup.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<AppDbContext>(options => {
                options.UseMySQL(Configuration["DbContext:ConnectionString"]);
            });

            // 添加redis
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // 添加swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlashMusic API", Version = "v1" });
            });

            // 允许跨域
            services.AddCors(options =>
            {
                options.AddPolicy(AllowCors,
                    builder =>
                    {
                        builder.AllowAnyMethod()
                            .AllowAnyOrigin()
                            .AllowAnyHeader();
                    });
            });

            // 注意AddTransient、AddSingleton、AddScoped的区别，注册服务
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IShoppingRepository, ShoppingRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            // app.UseHttpsRedirection();
            // 添加Swagger有关中间件
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlashMusic API v1");
            });


            // 你在哪
            app.UseRouting();

            // 你是谁
            app.UseAuthentication();

            // 你有什么权限
            app.UseAuthorization();

            app.UseCors(AllowCors);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
