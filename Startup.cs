using System;
using System.IO;
using System.Reflection;
using Dapper;
using Dapper.FluentMap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql;


// Start Up 主要負責應用程式的設定與中介軟體 (Middleware) 的設定
// ConfigureServices 是設定「什麼」服務可用
public class Startup
{
    // 接受 Iconfiguration 實例，用來讀取 appsetting.json 等環境變數的設定內容˙
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // 註冊 MVC 架構的控制器，允許 API 端點處理 HTTP 請求
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        string clientID = Configuration["APS_CLIENT_ID"];
        string clientSecret = Configuration["APS_CLIENT_SECRET"];
        string callbackURL = Configuration["APS_CALLBACK_URL"];
        if (
            string.IsNullOrEmpty(clientID)
            || string.IsNullOrEmpty(clientSecret)
            || string.IsNullOrEmpty(callbackURL)
        )
        {
            throw new ApplicationException(
                "Missing required environment variables APS_CLIENT_ID, APS_CLIENT_SECRET, or APS_CALLBACK_URL."
            );
        }
        services.AddSingleton(new APS(clientID, clientSecret, callbackURL));

        // 註冊必要服務
        services.AddScoped<ICustomerRepository, CustomerRepositoryImpl>();
        services.AddScoped<ICustomerService, CustomerServiceImpl>();

        services.AddScoped<IUnitRepository, UnitRepositoryImpl>();
        services.AddScoped<IUnitService, UnitServiceImpl>();

        services.AddScoped<IChangeDetailRepository, ChangeDetailRepositoryImpl>();
        services.AddScoped<IChangeDetailService, ChangeDetailServiceImpl>();

        services.AddScoped<IProjectRepository, ProjectRepositoryImpl>();
        services.AddScoped<IProjectService, ProjectServiceImpl>();

        services.AddScoped<IEditFilterRepository, EditFilterRepositoryImpl>();
        services.AddScoped<IEditFilterService, EditFilterServiceImpl>();
        // // 配置 Dapper 的全局映射
        // DefaultTypeMap.MatchNamesWithUnderscores = true;

        // 註冊 FluentMap，用來將映射類別註冊到 Dapper 中
        FluentMapper.Initialize(config =>
        {
            config.AddMap(new UnitMap());
            config.AddMap(new ChangeDetailMap());
            config.AddMap(new ProjectMap());
        });

        ILogger logger = LoggerFactory
            .Create(builder =>
            {
                builder.AddConsole();
            })
            .CreateLogger<Startup>();

        services.AddDbContext<DbContext>(options =>
            options.UseMySql(
                Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 21))
            )
        );

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "APS API for CEC R&D",
                    Version = "v1",
                    Description = "API for APS Hubs Browser application",
                }
            );
            // 設置 XML 文檔路徑
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowNextJS",
                builder =>
                    builder
                        .WithOrigins("http://localhost:3000", "http://localhost:5000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
            );
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    // 設定 HTTP 處理管道
    // 如果是要「使用」或「啟用」服務，放在 Configure
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 若處在開發階段，則啟用開發者錯誤頁，提供錯誤的詳細資訊，方便除錯
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "APS Hubs Browser API V1");
            });
        }
        // 靜態檔案路由設定
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowNextJS"); // 在 UseRouting 之後，UseEndpoints 之前
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
