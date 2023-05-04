using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()//signalR authenticating to the server
    .WithOrigins("http://localhost:4200"));

app.UseAuthentication(); // do you have a valid token
app.UseAuthorization(); // authorize the endpoint you can go


app.MapControllers();

app.MapHub<PresenceHub>("hubs/presence"); //how do the client find the hub(give a route)
app.MapHub<MessageHub>("hubs/message"); //how do the client find the hub(give a route)
using var scope = app.Services.CreateScope();
var services=scope.ServiceProvider;
try{
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    //context.Connections.RemoveRange(context.Connections); //this is for small scale database
    //await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
    //truncate is good, but for sqlite specificly,not good
    //be careful using this, directly using sql without using entityframework
    await Seed.SeedUsers(userManager,roleManager);

}
catch(Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex,"An error occurred during migration");
}

app.Run();
