﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build
{
    class OliveSolution : Builder
    {
        DirectoryInfo Root;
        DirectoryInfo Lib;

        bool Publish;

        public OliveSolution(DirectoryInfo root, bool publish)
        {
            Root = root;
            Publish = publish;
            Lib = root.CreateSubdirectory(@"M#\lib\netcoreapp2.1\");
        }

        protected override void AddTasks()
        {
            Add(() => BuildRuntimeConfigJson());
            Add(() => BuildMSharpModel());
            Add(() => MSharpGenerateModel());
            Add(() => BuildAppDomain());
            Add(() => BuildMSharpUI());
            Add(() => MSharpGenerateUI());
            Add(() => YarnInstall());
            Add(() => InstallBowerComponents());
            Add(() => TypescriptCompile());
            Add(() => SassCompile());
            Add(() => BuildAppWebsite());
        }

        void BuildRuntimeConfigJson()
        {
            var json = @"{  
   ""runtimeOptions"":{  
      ""tfm"":""netcoreapp2.1"",
      ""framework"":{  
         ""name"":""Microsoft.NETCore.App"",
         ""version"":""2.1.0""
      }
   }
}";
            File.WriteAllText(Path.Combine(Lib.FullName, "MSharp.DSL.runtimeconfig.json"), json);
        }

        static string DotnetBuildOptions => "-v q";

        void BuildMSharpModel()
        {
            var log = WindowsCommand.DotNet.Execute($"build {DotnetBuildOptions} \"{Folder("M#\\Model")}\"");
            Log(log);
        }


        void BuildAppDomain()
        {
            var log = WindowsCommand.DotNet.Execute($"build {DotnetBuildOptions} \"{Folder("Domain")}\"");
            Log(log);
        }

        void BuildMSharpUI()
        {
            var log = WindowsCommand.DotNet.Execute($"build {DotnetBuildOptions} \"{Folder("M#\\UI")}\"");
            Log(log);
        }

        void BuildAppWebsite()
        {
            var command = "build " + DotnetBuildOptions;
            if (Publish) command = "publish -o publish";

            var log = WindowsCommand.DotNet.Execute(command,
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));

            Log(log);
        }

        void MSharpGenerateModel()
        {
            var log = WindowsCommand.DotNet.Execute($"msharp.dsl.dll /build /model /no-domain",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("M#\\lib\\netcoreapp2.1"));
            Log(log);
        }

        void MSharpGenerateUI()
        {
            var log = WindowsCommand.DotNet.Execute($"msharp.dsl.dll /build /ui",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("M#\\lib\\netcoreapp2.1"));
            Log(log);
        }

        void YarnInstall()
        {
            var log = WindowsCommand.Yarn.Execute("install",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void InstallBowerComponents()
        {
            var log = WindowsCommand.Bower.Execute("install",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void TypescriptCompile()
        {
            var log = WindowsCommand.TypeScript.Execute("",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void SassCompile()
        {
            var log = Folder("Website\\wwwroot\\Styles\\Build\\SassCompiler.exe")
                 .AsFile()
                 .Execute("\"" + Folder("Website\\CompilerConfig.json") + "\"");

            Log(log);
        }

        string Folder(string relative) => Path.Combine(Root.FullName, relative);
    }
}
