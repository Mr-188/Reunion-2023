using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReunionServer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc().AddNewtonsoftJson(config =>
{
    config.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    config.SerializerSettings.Converters.Add(new StringEnumConverter());
    config.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
builder.Services.AddControllers();
var mysql = builder.Configuration.GetValue<string>("Database");
builder.Services.AddDbContext<DataContext>(c =>
{
    c.UseMySql(mysql, ServerVersion.AutoDetect(mysql));
    if (builder.Environment.IsDevelopment())
    {
        c.EnableSensitiveDataLogging();
    }
});

var app = builder.Build();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetService<DataContext>();
    dataContext!.Database.EnsureCreated();
    dataContext.Database.Migrate();
}

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();