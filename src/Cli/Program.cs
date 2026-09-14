// See https://aka.ms/new-console-template for more information

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

var envData = new
{
    OSDescription = RuntimeInformation.OSDescription,
    EnvironmentOS = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    FrameworkDescription = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory
};

if (args.Contains("--json"))
{   
     var options = new JsonSerializerOptions 
    { 
        WriteIndented = true 
    };
    string json = JsonSerializer.Serialize(envData,options);
    Console.WriteLine(json);
}
else
{
    Console.WriteLine("================================================================================");
    Console.WriteLine($"| {"Параметр",-25} | {"Значення",-48} |");
    Console.WriteLine("================================================================================");
    Console.WriteLine($"| {"ОС (OSDescription)",-25} | {envData.OSDescription,-48} |");
    Console.WriteLine($"| {"ОС (Environment)",-25} | {envData.EnvironmentOS,-48} |");
    Console.WriteLine($"| {"Архітектура процесора",-25} | {envData.ProcessArchitecture,-48} |");
    Console.WriteLine($"| {"Версія .NET (CLR)",-25} | {envData.DotNetVersion,-48} |");
    Console.WriteLine($"| {"Runtime",-25} | {envData.FrameworkDescription,-48} |");
    Console.WriteLine($"| {"Каталог застосунку",-25} | {envData.BaseDirectory,-48} |");
    Console.WriteLine($"| {"Поточний каталог",-25} | {envData.CurrentDirectory,-48} |");
    Console.WriteLine("================================================================================");
}