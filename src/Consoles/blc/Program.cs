using System.Text;

using LF.Compiler;
using LF.Compiler.Assembly;
using LF.Compiler.C;
using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Code.Instruction;
using LF.Compiler.Debugging;
using LF.Compiler.Logging;
using LF.Compiler.Mapper;
using LF.Compiler.Reader;

using LFC.Commandline;
using LFC.Make;

namespace LFC;

internal static class Program
{
    private static string ProjectTemplatePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "project_template");

    private static void CreateProject(string dir, string templateType, bool forceOverwrite)
    {
        string templateDir = Path.Combine(ProjectTemplatePath, templateType);
        if (!Directory.Exists(templateDir))
        {
            throw new Exception($"Template directory {templateDir} does not exist");
        }

        string[] files = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string destFile = Path.Combine(dir, file.Substring(templateDir.Length + 1));
            if (File.Exists(destFile))
            {
                throw new Exception($"File {destFile} already exists");
            }

            string destDir = Path.GetDirectoryName(destFile)!;
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(file, destFile, forceOverwrite);
        }
    }

    private static void EnsureParentExists(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file))!);
    }

    private static void WriteAssemblyInfo(string file)
    {
        LFAssemblyMapper mapper = new LFAssemblyMapper();

        try
        {
            LFAssembly asm = LFAssembly.FromFile(file);
            asm.Initialize(0x8000, Path.GetDirectoryName(Path.GetFullPath(file))!, mapper);

            Logger.Info(mapper.GetInformation());
        }
        catch (Exception e)
        {
            Logger.Error($"Could not load Assembly: {e.Message}");
        }
    }

    private static void CheckAssembly(string outFile)
    {
        LFAssembly asm = LFAssembly.FromFile(outFile);
        LFAssemblyMapper mapper = new LFAssemblyMapper();
        try
        {
            asm.Initialize(0x8000, Path.GetDirectoryName(Path.GetFullPath(outFile))!, mapper);

            mapper.PatchAssemblies();
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error("Assembly loading Failed! Do all dynamic libraries exist?");
        }
    }

    public static bool Compile(LFCompilerArguments args, string inFile, string outFile)
    {
        LFReaderResult result = LFCodeParser.Parse(Path.GetFileNameWithoutExtension(outFile), inFile);

        if (args.DoNotWriteEmpty && result.IsEmpty)
        {
            return false;
        }


        MemoryStream ms = new MemoryStream();
        LFAssemblyDebugSymbolDatabase symbolDatabase = new LFAssemblyDebugSymbolDatabase();
        result.Serialize(ms, symbolDatabase);
        byte[] data = ms.ToArray();

        EnsureParentExists(outFile);
        File.WriteAllBytes(outFile, data);

        if (!args.DoNotExportSymbols)
        {
            Logger.Info("Exporting Symbols to " + outFile + ".sym");
            File.WriteAllText(outFile + ".sym", symbolDatabase.Export());
        }

        Logger.Info($"Output Binary: {outFile}");

        return true;
    }

    private static void PrintHelp()
    {
        Logger.Info("LFC - LF Compiler");
        Logger.Info("Help: LFC.exe help");
        Logger.Info("Compile: LFC.exe <inputFile> <outputFile>.lfbin");
        Logger.Info("Compile All Files in: LFC.exe <inputDir> <outputDir>");
        Logger.Info("Run Make File: LFC.exe make [makeFile] [target]");
        Logger.Info("Create Project: LFC.exe new <templateType> [projectDir]");
        Logger.Info("Assembly Info: LFC.exe <file>.lfbin");

        string text = string.Join("\n\t", CommandlineParser<LFCompilerArguments>.GetHelpText().Split('\n'));
        Logger.Info($"Commandline Options: \n\t{text}");
    }

    private static string SelectTarget(string[] targets)
    {
        Console.WriteLine("Select a Target to Run:");
        foreach (string target in targets)
        {
            Console.WriteLine($"\t{target}");
        }

        string? input = null;
        while (input == null || !targets.Contains(input))
        {
            Console.Write("Input: ");
            input = Console.ReadLine();
        }

        return input;
    }

    private static void GenerateWrappers()
    {
        StringBuilder instrSb = new StringBuilder();

        instrSb.AppendLine("namespace Instructions");
        instrSb.AppendLine("{");

        instrSb.AppendLine("");
        instrSb.AppendLine("\t//Instruction Keys");
        instrSb.AppendLine("");
        string[] instructions = Enum.GetNames(typeof(OpCodes));
        foreach (string instruction in instructions)
        {
            OpCodes op = Enum.Parse<OpCodes>(instruction);
            instrSb.AppendLine($"\t#export {instruction}");
            instrSb.AppendLine($"\ti16 {instruction}()");
            instrSb.AppendLine("\t{");
            instrSb.AppendLine($"\t\treturn {(ushort)op};");
            instrSb.AppendLine("\t}");
            instrSb.AppendLine("");

        }
        instrSb.AppendLine("");
        instrSb.AppendLine("\t//Wrapper Functions");
        instrSb.AppendLine("");
        
        instrSb.AppendLine("\t#export GetInstructionName");
        instrSb.AppendLine("\ti8* GetInstructionName(i16 opcode)");
        instrSb.AppendLine("\t{");
        foreach (string instruction in instructions)
        {
            OpCodes op = Enum.Parse<OpCodes>(instruction);
            instrSb.AppendLine($"\t\tif (opcode == {(ushort)op})");
            instrSb.AppendLine("\t\t{");
            instrSb.AppendLine($"\t\t\treturn \"{instruction}\";");
            instrSb.AppendLine("\t\t}");
        }
        instrSb.AppendLine("\t\treturn \"Unknown\";");
        instrSb.AppendLine("\t}");
        instrSb.AppendLine("");
        
        instrSb.AppendLine("\t#export IsValidInstruction");
        instrSb.AppendLine("\ti8 IsValidInstruction(i16 opcode)");
        instrSb.AppendLine("\t{");
        foreach (string instruction in instructions)
        {
            OpCodes op = Enum.Parse<OpCodes>(instruction);
            instrSb.AppendLine($"\t\tif (opcode == {(ushort)op})");
            instrSb.AppendLine("\t\t{");
            instrSb.AppendLine($"\t\t\treturn 1;");
            instrSb.AppendLine("\t\t}");
        }
        instrSb.AppendLine("\t\treturn 0;");
        instrSb.AppendLine("\t}");
        instrSb.AppendLine("");
        
        instrSb.AppendLine("\t#export GetInstructionArgumentCount");
        instrSb.AppendLine("\ti8 GetInstructionArgumentCount(i16 opcode)");
        instrSb.AppendLine("\t{");
        foreach (string instruction in instructions)
        {
            OpCodes op = Enum.Parse<OpCodes>(instruction);
            instrSb.AppendLine($"\t\tif (opcode == {(ushort)op})");
            instrSb.AppendLine("\t\t{");
            instrSb.AppendLine($"\t\t\treturn {LFAssemblyInstructionInfo.GetArgumentCount(op)};");
            instrSb.AppendLine("\t\t}");
        }
        instrSb.AppendLine("\t\treturn 0;");
        instrSb.AppendLine("\t}");
        instrSb.AppendLine("");
        
        instrSb.AppendLine("\t#export GetInstructionArgumentSize");
        instrSb.AppendLine("\ti8 GetInstructionArgumentSize(i16 opcode, i8 argIndex)");
        instrSb.AppendLine("\t{");
        foreach (string instruction in instructions)
        {
            OpCodes op = Enum.Parse<OpCodes>(instruction);
            instrSb.AppendLine($"\t\tif (opcode == {(ushort)op})");
            instrSb.AppendLine("\t\t{");
            for (int i = 0; i < LFAssemblyInstructionInfo.GetArgumentCount(op); i++)
            {
                instrSb.AppendLine($"\t\t\tif (argIndex == {i})");
                instrSb.AppendLine("\t\t\t{");
                instrSb.AppendLine($"\t\t\t\treturn {LFAssemblyInstructionInfo.GetArgumentSize(op, i)};");
                instrSb.AppendLine("\t\t\t}");
            }
            instrSb.AppendLine($"\t\t\treturn 0;");
            instrSb.AppendLine("\t\t}");
        }
        instrSb.AppendLine("\t\treturn 0;");
        instrSb.AppendLine("\t}");
        instrSb.AppendLine("");
        
        instrSb.AppendLine("}");
        
        string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "templates", "Instructions.lfc");
        File.WriteAllText(outDir, instrSb.ToString());
    }
    
    internal static void Main(string[] args)
    {
        LFMakeSystem.OnTargetSelect += SelectTarget;
        while (args.Length == 0)
        {
            Console.WriteLine("Commandline Input: ");
            string input = Console.ReadLine()!;
            args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        CommandlineParseResult<LFCompilerArguments> result = CommandlineParser<LFCompilerArguments>.Parse(args);

        if (!result.Object.NoLogs)
        {
            Logger.SetLogger(new TagLogger(new LFLogger(), "LFC"));
        }


        LFCompilerOptimizationSettings.Instance.OptimizeAll = result.Object.Optimize;

        LFAssemblyParser.AddParser();
        LFCParser.AddParser();

        string inFile;
        string outFile;


        if (result.RemainingArguments.Length >= 1 && result.RemainingArguments[0] == "init")
        {
            // Generate Wrappers for all Instructions
            GenerateWrappers();
            
            
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "projects");
            string? target = null;
            if (result.RemainingArguments.Length >= 2)
            {
                target = result.RemainingArguments[1];
            }

            foreach (string project in Directory.GetDirectories(projectDir, "*", SearchOption.TopDirectoryOnly))
            {
                string makeFile = Path.Combine(project, "project.json");
                LFMakeSystem.Make(new LFCompilerArguments(), makeFile, target);
            }

            return;
        }

        if (result.RemainingArguments.Length > 1 && result.RemainingArguments[0] == "new")
        {
            string templateType = result.RemainingArguments[1];
            string dir = Directory.GetCurrentDirectory();
            if (result.RemainingArguments.Length == 3)
            {
                dir = result.RemainingArguments[2];
            }

            CreateProject(dir, templateType, result.Object.ForceOverwrite);

            return;
        }

        if (result.RemainingArguments.Length > 0 && result.RemainingArguments[0] == "make")
        {
            string file = Path.Combine(Directory.GetCurrentDirectory(), "project.json");
            string? target = null;
            if (result.RemainingArguments.Length > 1)
            {
                if (result.RemainingArguments[1].EndsWith(".json"))
                {
                    file = result.RemainingArguments[1];
                    if (result.RemainingArguments.Length > 2)
                    {
                        target = result.RemainingArguments[2];
                    }
                }
                else
                {
                    target = result.RemainingArguments[1];
                }
            }

            LFMakeSystem.Make(result.Object, file, target);

            return;
        }

        if (result.RemainingArguments.Length == 1 && Path.GetExtension(result.RemainingArguments[0]) == ".lfbin")
        {
            WriteAssemblyInfo(result.RemainingArguments[0]);

            return;
        }

        if (result.RemainingArguments.Length == 1 && result.RemainingArguments[0] == "help")
        {
            PrintHelp();

            return;
        }

        if (result.RemainingArguments.Length != 2)
        {
            PrintHelp();

            return;
        }

        inFile = result.RemainingArguments[0];
        outFile = result.RemainingArguments[1];

        List<Exception> errors = new List<Exception>();

        if (Directory.Exists(inFile))
        {
            string[] files = Directory.GetFiles(inFile, "*", SearchOption.AllDirectories);
            string[] outFiles = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                outFiles[i] = Path.ChangeExtension(files[i].Replace(inFile, outFile), ".lfbin");
            }

            bool[] success = new bool[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string inF = files[i];
                string outF = outFiles[i];
                try
                {
                    success[i] = Compile(result.Object, inF, outF);
                }
                catch (Exception e)
                {
                    errors.Add(e);
                    Logger.Error($"Error: {e.Message}");
                }
            }

            for (int i = 0; i < files.Length; i++)
            {
                string outF = outFiles[i];
                if (!result.Object.DoNotCheckOutput && success[i])
                {
                    CheckAssembly(outF);
                }
            }
        }
        else
        {
            bool success = false;
            try
            {
                success = Compile(result.Object, inFile, outFile);
            }
            catch (Exception e)
            {
                errors.Add(e);
                Logger.Error($"Error: {e.Message}");
            }

            if (!result.Object.DoNotCheckOutput && success)
            {
                CheckAssembly(outFile);
            }
        }

        if (errors.Count > 0)
        {
            if (result.Object.ThrowExceptions)
            {
                throw new AggregateException(errors);
            }

            if (errors.Count == 0)
            {
                Logger.Info($"{errors.Count} Errors:");
            }
            else
            {
                Logger.Error($"{errors.Count} Errors:");
            }

            foreach (Exception exception in errors)
            {
                Logger.Error($"Error: {exception.Message}");
            }
        }
    }
}