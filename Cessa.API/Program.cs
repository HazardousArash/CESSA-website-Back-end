using Cessa.Application;
using Cessa.Infrastructure;
using Cessa.Persistence;
using Cessa.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Cessa.Domain.Entities.Auth;
using Cessa.Api.Infrastructure.Security;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddCessaRateLimiting();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Cessa.Persistence.Contexts.ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    try { await dbContext.Database.MigrateAsync(); } catch { }

    try
    {
        await dbContext.Database.ExecuteSqlRawAsync(@"
            IF COL_LENGTH('HallOfFameMembers', 'EntryYear') IS NULL ALTER TABLE HallOfFameMembers ADD EntryYear int NULL;
            IF COL_LENGTH('HallOfFameMembers', 'IsAssociationMember') IS NULL ALTER TABLE HallOfFameMembers ADD IsAssociationMember bit NOT NULL DEFAULT 0;
            IF COL_LENGTH('HallOfFameMembers', 'Term') IS NULL ALTER TABLE HallOfFameMembers ADD Term nvarchar(100) NULL;
            IF COL_LENGTH('HallOfFameMembers', 'RoleTitle') IS NULL ALTER TABLE HallOfFameMembers ADD RoleTitle nvarchar(100) NULL;
            
            IF OBJECT_ID('PostCategories', 'U') IS NULL CREATE TABLE PostCategories (Id UNIQUEIDENTIFIER PRIMARY KEY, Name NVARCHAR(200) NOT NULL, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NULL, IsDeleted BIT NOT NULL DEFAULT 0);
            
            IF COL_LENGTH('Posts', 'CategoryId') IS NULL BEGIN 
                ALTER TABLE Posts ADD CategoryId UNIQUEIDENTIFIER NULL; 
                ALTER TABLE Posts ADD CONSTRAINT FK_Posts_Category FOREIGN KEY (CategoryId) REFERENCES PostCategories(Id); 
            END
            
            IF OBJECT_ID('UserCategoryPreferences', 'U') IS NULL CREATE TABLE UserCategoryPreferences (Id UNIQUEIDENTIFIER PRIMARY KEY, UserId UNIQUEIDENTIFIER NOT NULL, CategoryId UNIQUEIDENTIFIER NOT NULL, ClickCount INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NULL, IsDeleted BIT NOT NULL DEFAULT 0, CONSTRAINT FK_UCP_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id), CONSTRAINT FK_UCP_Category FOREIGN KEY (CategoryId) REFERENCES PostCategories(Id));
        ");
    }
    catch { }

    await Cessa.Persistence.Seeders.DemoDataSeeder.SeedAsync(dbContext, userManager, roleManager, app.Configuration);
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwaggerInProd"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/public/swagger.json", "Public API");
        c.SwaggerEndpoint("/swagger/student/swagger.json", "Student API");
        c.SwaggerEndpoint("/swagger/contentcreator/swagger.json", "Content Creator API");
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin API");
    });
}

app.UseExceptionHandler();

var uploadDir = app.Configuration["UploadSettings:Directory"] ?? "./uploads";
var absoluteUploadFolder = Path.GetFullPath(uploadDir);
if (!System.IO.Directory.Exists(absoluteUploadFolder)) System.IO.Directory.CreateDirectory(absoluteUploadFolder);

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(absoluteUploadFolder),
    RequestPath = "/uploads"
});

app.UseRateLimiter();
app.UseMiddleware<Cessa.Api.Infrastructure.Security.SecurityHeadersMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }
