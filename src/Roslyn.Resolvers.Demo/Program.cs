using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslyn.Resolvers.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var code = File.ReadAllText("main.csx");
            var opts = ScriptOptions.Default.
                AddImports("System").
                WithSourceResolver(new RemoteFileResolver());

            var script = CSharpScript.Create(code, opts);
            var compilation = script.GetCompilation();
            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any())
            {
                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine(diagnostic.GetMessage());
                }
            }
            else
            {
                var result = script.RunAsync().Result;
            }

            Console.ReadKey();
        }
    }
}
