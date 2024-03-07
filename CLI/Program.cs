using System.CommandLine;
using System;
using System.IO;
using System.Linq;

using System.CommandLine.Invocation;

static string[] ChooseLanguages(string[] languages, string[] extensions, string myLanguages)
{
    if (myLanguages == "all")
        return extensions;
    string[] result = myLanguages.Split(' ');

    for (int i = 0; i < result.Length; i++)
    {
        for (int j = 0; j < languages.Length; j++)
        {
            if (result[i] == languages[j])
            {
                result[i] = extensions[j];
            }
        }
    }
    return result;
}

string[] languages = { "c#", "java", "python", "html", "c++", "c", "javaScript", "css" };
string[] extentsions = { ".cs", ".java", ".py", ".html", ".cpp", ".c", ".js", ".css" };

var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var outputOption = new Option<FileInfo>(new[] {"--output","-o"}, "File path and name");
var languageOption = new Option<string>(new[] { "--language", "-l" }, "list of languages") { IsRequired=true};
var noteOption = new Option<bool>(new[] { "--note", "-n" }, () => false, "note the source code");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "the author that create the file");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, "the auther that create the file");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, () => false, "delete empty lines");

bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeEmptyLinesOption);
sortOption.SetDefaultValue("letter");
bundleCommand.AddOption(sortOption);

bundleCommand.SetHandler((output, language, note, author, sort, remove) =>
{
    string[] arrlanguages = ChooseLanguages(languages, extentsions, language);
    List<string> Folders = Directory.GetFiles(Directory.GetCurrentDirectory(), "", SearchOption.AllDirectories).Where(p => !p.Contains("bin") && !p.Contains("Debug")).ToList();
    string[] files = Folders.Where(f => arrlanguages.Contains(Path.GetExtension(f))).ToArray();

    try
    {
        if (files.Length > 0)
        {
            using (var file = new StreamWriter(output.FullName, false))
            {
                
                if (!string.IsNullOrEmpty(author))
                    file.WriteLine("#Author: " + author);
                
                if (note)
                   file.WriteLine($"# Source code from: {Directory.GetCurrentDirectory()}\n");

                foreach (var f in files)
                {
                    if (note)
                        file.WriteLine($"# Source code from: {f}\n");

                    var lines = File.ReadAllLines(f);
                    if (remove)
                        lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

                    //if (sort)
                    //    Array.Sort(files);
                    if (sort.ToLower() == "type")
                    {
                        files = files.OrderBy(code => Path.GetExtension(code)).ToArray();
                    }
                    else
                    {
                        files = files.OrderBy(code => Path.GetFileNameWithoutExtension(code)).ToArray();
                    }

                    foreach (var line in lines)
                        file.WriteLine(line);

                    file.WriteLine();
                }
            }
        } 
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("error path is invalid");
    }
}, outputOption, languageOption, noteOption, authorOption, sortOption, removeEmptyLinesOption);


var rspCommand = new Command("create-rsp", "Bundle code files to a single file");

rspCommand.SetHandler(() =>
{
   
    string result="bundle";

    Console.WriteLine("Enter the name of the file");
    result += $" -o {Console.ReadLine()}.txt";

    Console.WriteLine("Enter list of languages or all to all the files");
    result += $" -l {Console.ReadLine()}";

    Console.WriteLine("Do you want to note the source code? (y/n)");
    if (Console.ReadLine() == "y")
        result += $" -n";

    Console.WriteLine("Enter the author that create the file");
    result+=$" -a {Console.ReadLine()}";

    Console.WriteLine("Do you want to delete empty lines? (y/n)");
    if (Console.ReadLine() == "y")
        result += " -r";

    Console.WriteLine("Do you want to sort by letter or type? (l/t)");
    if (Console.ReadLine() == "l") 
       result += " -s letter";
    else result+=" -s type";

    File.WriteAllText("responseFile.rsp" ,result);
    
});


var rootCommand = new RootCommand("Root command for file Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(rspCommand);
rootCommand.InvokeAsync(args);

