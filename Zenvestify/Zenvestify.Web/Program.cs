using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Zenvestify.Shared.Services;
using Zenvestify.Web.Components;
using Zenvestify.Web.Configs;
using Zenvestify.Web.Data;
using Zenvestify.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


//EF
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "Zenvestify.Web", Version = "v1" });

	// Add JWT auth option in Swagger
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxYmQ0YTljZi1jMmMyLTQ1ZDQtYjVlNC0wYTNlNzQwYjc5NWIiLCJlbWFpbCI6ImFzaW1AdGVzdGVyLmNvbSIsIkZ1bGxOYW1lIjoiQXNpbSBGYWlheiBUZXN0ZXIiLCJleHAiOjE3NTU5NTEwNDgsImlzcyI6IlplbnZlc3RpZnlBUEkiLCJhdWQiOiJaZW52ZXN0aWZ5Q2xpZW50In0.RBlQQ0IiNkZIgOrsSUgnK_GvzxlX8ZugEkG6K0gF7vw"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the Zenvestify.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Services.AddHttpClient();

//Json Token builder
//var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
	var jwtSection = builder.Configuration.GetSection("Jwt");
	var jwtOptions = jwtSection.Get<JwtOptions>();

	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtOptions.Issuer,
		ValidAudience = jwtOptions.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
	};
});

//Register service (These two maybe delete later)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserProfileService>();


builder.Services.AddScoped<UserRepository>();

var app = builder.Build();


app.Use(async (context, next) =>
{
	var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
	Console.WriteLine($"[SERVER.Middleware] Incoming {context.Request.Method} {context.Request.Path} | Auth={authHeader}");
	await next();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Zenvestify.Shared._Imports).Assembly,
        typeof(Zenvestify.Web.Client._Imports).Assembly);

app.Run();
