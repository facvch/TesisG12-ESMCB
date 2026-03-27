using Application;
using Application.Registrations;
using AutoMapper;
using Core.Application;
using Filters;
using Infrastructure.Registrations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Text;

namespace API
{
    public class Startup
    {
        public IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddApplicationServices();
            services.AddInfrastructureServices(Configuration);

            // Response Compression (gzip + brotli)
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/json", "text/csv", "text/markdown"
                });
            });
            services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

            // JWT Authentication
            var jwtKey = Configuration["Jwt:Key"] ?? "VeterinariaSecretKeyMuyLargaParaDesarrollo2024!@#$";
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"] ?? "VeterinariaAPI",
                    ValidAudience = Configuration["Jwt:Audience"] ?? "VeterinariaApp",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    RoleClaimType = "role",
                    NameClaimType = "name"
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT Auth Failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Veterinaria API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Token. Formato: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            services.AddMvc().AddMvcOptions(options =>
            {
                options.Filters.Add<BaseExceptionFilter>();
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            CustomMapper.Instance = app.ApplicationServices.GetRequiredService<IMapper>();

            app.UseMiddleware<API.Middleware.PerformanceMiddleware>();
            app.UseMiddleware<Middleware.GlobalExceptionMiddleware>();
            app.UseMiddleware<API.Middleware.RateLimitingMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<API.Middleware.AuditMiddleware>();
            //UseEventBus(app);
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private void UseEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            // Aqui se registran las subscripciones a eventos del bus de eventos, vinculando
            //eventos con sus respectivos handlers
            eventBus.Subscribe<DummyEntityCreatedIntegrationEvent, DummyEntityCreatedIntegrationEventHandlerSub>();
        }
    }
}
