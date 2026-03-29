using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

class Program {
    static void Main() {
        var services = new ServiceCollection();
        services.AddGraphQLServer().AddAuthorization();
        var sp = services.BuildServiceProvider();
        var handler = sp.GetService<HotChocolate.Authorization.IAuthorizationHandler>();
        Console.WriteLine("Handler: " + (handler != null ? handler.GetType().FullName : "null"));
    }
}
