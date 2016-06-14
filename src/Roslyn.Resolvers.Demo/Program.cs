using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;

namespace Roslyn.Resolvers.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var code = @"
                #load ""https://gist.githubusercontent.com/filipw/9a79bb00e4905dfb1f48757a3ff12314/raw/adbfe5fade49c1b35e871c49491e17e6675dd43c/foo.csx""
                #load ""foo.csx""
                Console.WriteLine(""Hello"");
            ";

            var opts = ScriptOptions.Default.
                AddImports("System").
                WithSourceResolver(new RemoteFileResolver(ImmutableArray.Create(new string[0]), 
                AppContext.BaseDirectory));

            var script = CSharpScript.Create(code, opts);
            var result = script.RunAsync().Result;

            Console.ReadKey();
        }
    }
}
