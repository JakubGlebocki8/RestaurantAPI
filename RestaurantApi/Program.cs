using RestaurantAPI;
using RestaurantAPI.Entities;
using RestaurantAPI.Services;
using System.Reflection;
using NLog.Web;
using RestaurantAPI.Middleware;
using static RestaurantAPI.Services.AccountService;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using FluentValidation.AspNetCore;
using Microsoft.IdentityModel.Tokens; 
using System.Text;
using RestaurantAPI.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = "Bearer";
    option.DefaultScheme = "Bearer";
    option.DefaultChallengeScheme = "Bearer";
})
    .AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = authenticationSettings.JwtIssuer,
        ValidAudience = authenticationSettings.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality","Polish","German"));
    options.AddPolicy("AtLesat20",builder => builder.AddRequirements(new MinimumAgeRequirement(20)));
    options.AddPolicy("CreatedAtleast2Restaurant", builder => builder.AddRequirements(new CreatedMultipleRestaurantsRequirement(2)));
});
builder.Services.AddScoped<IUserContextService, UserContextService>(); 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler,MinimumAgeRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler,CreatedMultipleRestaurantsRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler,ResourceOperationRequirementHandler>();
builder.Services.AddScoped<IValidator<RestaurantQuery>,RestaurantQueryValidator>();
var appsetinsgs = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndClient", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());


});
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddDbContext<ResturantDbContext>();
builder.Services.AddScoped<RestaurantSeeder>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
builder.Services.AddScoped<IPasswordHasher<User>,PasswordHasher<User>>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IDishService,DishService>();
builder.WebHost.UseNLog();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<RequestTimeMiddleware>();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.UseResponseCaching();
app.UseStaticFiles();
var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();


// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeMiddleware>();
app.UseCors("FrontEndClient");
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json","Restaurant API"));
seeder.Seed();

app.UseAuthorization();

app.MapControllers();


app.Run();
