using Microsoft.AspNetCore.Builder;
using HotChocolate.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapGraphQL();

app.Run();

public class Query
{
    [Authorize]
    public string GetHello() => "Hello World";
}
