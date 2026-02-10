using Assets;
using Authorization;
using BuildingBlocks.Caching.Extensions;
using BuildingBlocks.Database;
using BuildingBlocks.Database.Extensions;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Search.Extensions;
using Identity;
using Menus;
using Microsoft.OpenApi.Models;
using Users;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.Host.AddLogging();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Asset Management API",
        Version = "v1",
        Description = "API for Asset Management System with RBAC"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Database
builder.Services.AddDatabase(builder.Configuration);

// Add Redis Cache
builder.Services.AddRedisCache(builder.Configuration);

// Add Elasticsearch
builder.Services.AddElasticsearch(builder.Configuration);

// Add Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddAuthorizationModule();
builder.Services.AddAssetsModule();
builder.Services.AddUsersModule();
builder.Services.AddMenusModule();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();
