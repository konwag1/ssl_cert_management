using Microsoft.EntityFrameworkCore;
using Namespace.Data;
using Microsoft.Data.SqlClient;
using Namespace.Services;  

var builder = WebApplication.CreateBuilder(args);


var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
{
    DataSource = "KONRADACER\\SQLEXPRESS",
    InitialCatalog = "SSLCertificatesDb",
    IntegratedSecurity = true,
    Encrypt = true,
    TrustServerCertificate = true
};

builder.Services.AddDbContext<SSLContext>(options =>
    options.UseSqlServer(sqlConnectionStringBuilder.ConnectionString));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        builder =>
        {
            builder.WithOrigins("https://localhost:7038")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


builder.Services.AddHostedService<CertificateMonitor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazorClient"); 
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
