using PRN222_Assignment4_Option1.BusinessLogic.Extensions;
using PRN222_Assignment4_Option1.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddExchangeRateServices(builder.Configuration);
builder.Services.AddSingleton<IWorkerControlService, WorkerControlService>();
builder.Services.AddHostedService<Worker>(); // Worker chạy cùng Web → dữ liệu mới liên tục
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ExchangeRate}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
