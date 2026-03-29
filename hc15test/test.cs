using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var asm in assemblies)
        {
            if(asm.FullName.Contains("HotChocolate") || asm.FullName.Contains("Microsoft.Extensions.DependencyInjection"))
            {
                var types = asm.GetTypes().Where(t => t.IsSealed && !t.IsGenericType && !t.IsNested);
                foreach(var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach(var method in methods)
                    {
                        if(method.Name == "AddAuthorization")
                        {
                            Console.WriteLine($"{asm.GetName().Name} -> {type.FullName}.{method.Name}");
                        }
                    }
                }
            }
        }
    }
}
