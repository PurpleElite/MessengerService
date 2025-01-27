using Microsoft.EntityFrameworkCore;
using MessengerService.Models;
using System.Reflection.Metadata;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<MessengerDbContext>(opt => opt.UseInMemoryDatabase("MessengerDb").UseSeeding((context, _) =>
{
    var mailAddress1 = "user1@test.com";
    var mailAddress2 = "user2@test.com";
    for (int i = 0; i < 10; i++)
    {
        context.Set<Message>().Add(new Message
        {
            ID = Guid.NewGuid(),
            RecipientAddress = i % 2 == 0 ? mailAddress1 : mailAddress2,
            SenderAddress = i % 2 == 0 ? mailAddress2 : mailAddress1,
            Content = $"This is a test message that pre-populates the in-memory database at runtime. It is test message number {i}.",
            SentTimestamp = new DateTime(2025, 1, 1+i),
            ReadTimestamp = i > 5 ? null : new DateTime(2025, 1, 2+i) 
        });
    }
    
    context.SaveChanges();
}));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MessengerDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();