using System;
using System.Text;
using Form_Project.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NZWalks.API.Repostre;
using Person.Data;
using Person.Data.Model;
using Person.Data.Repostry.PersonRepo;
using Person.Data.Repostry.UserRepo;
using Serilog;
using Serilog.Events;
using YourNamespace.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ✅ أضف هذا السطر لتفعيل IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

//Logeers in the File
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/Persons-app-.log", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();

//ضافة الفلتر لجميع Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FullLogActionFilter>();
    options.Filters.Add<UserIdLogActionFilter>();

});





builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Add DbContext
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Person_ConnectionStrings")));


// add the Identity User and Roles with id int
builder.Services.AddIdentity<Users, Roles>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<PersonDbContext>()
.AddDefaultTokenProviders();


// here we add interfeces
builder.Services.AddScoped<IPersonRepo, PersonsRepo>();
builder.Services.AddScoped<ITokenRepo, TokenRepo>();


// add Atou Maper 
builder.Services.AddAutoMapper(typeof(AtuoMap));


// Add Auth in the Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Person API", Version = "v1" });

    // إضافة JWT Authorization
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Description = "Enter the JWT token : Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            },
            Scheme="Bearer",
            Name="Authorization",
            In= ParameterLocation.Header
        },
        new List<string>()
    }
});
});


//_________________________________________________________________________________

// Add Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),
        ClockSkew = TimeSpan.Zero
    };

    // 👇 هذا السطر يطبع أي خطأ في JWT
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("❌ JWT Failed: " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});
//__________________________________________________________________________________________________________________________________________________


// Validation 
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errorMessages = context.ModelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        // بدلاً من إرجاع object نرجع array
        return new BadRequestObjectResult(errorMessages);
    };
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// for Paphish Images
var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
if (!Directory.Exists(imagesPath))
    Directory.CreateDirectory(imagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

Log.Information("Aplication Starting");

//app.Urls.Add("http://0.0.0.0:8080");
// Validation
app.UseMiddleware<ErrorHandlingMiddleware>();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
try
{
    Log.Information("Application starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}