using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyPhotos.Models;
using MyPhotos.Models.Data;
using System.Reflection;

namespace MyPhotos
{
    /// <inheritdoc/>
    public static class Program
    {
        /// <inheritdoc/>
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "MyPhotos.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
            {
                Name = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Укажите токен авторизации",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
            };

            OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Id = "jwt_auth",
                    Type = ReferenceType.SecurityScheme
                }
            };

            OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
            {
                { securityScheme, Array.Empty<string>() },
            };

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("jwt_auth", securityDefinition);
                options.AddSecurityRequirement(securityRequirements);

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "My Photos API",
                    Description = """
                    My Photos API предоставляет методы сохранения и просмотра изображений

                    После успешной регистрации пользователя, он сможет:
                    * Загружать свои фотографии и посматривать их;
                    * Добавлять в друзья других пользователей;
                    * Просматривать изображения своих друзей
                    """,
                    Contact = new OpenApiContact
                    {
                        Name = "Егор",
                        Email = "danchin276@mail.ru",
                        Url = new Uri("https://vk.com/ab0_obus69")
                    }
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
            
            string connectionString = builder.Configuration.GetConnectionString("MyPhotos")
                ?? throw new Exception("Не обнаружена строка подключения");

            builder.Services.AddDbContext<MyPhotosContext>(options
                => options.UseSqlServer(connectionString));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });


            builder.Services.AddAuthorization();

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCookiePolicy();
            app.UseSession();
            app.Use(async (context, next) =>
            {
                string? token = context.Session.GetString("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Add("Athorization", "Bearer " + token);
                }

                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}