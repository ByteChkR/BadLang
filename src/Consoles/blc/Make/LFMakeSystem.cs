using System.Diagnostics;

using LF.Compiler.Logging;

using Newtonsoft.Json.Linq;

namespace LFC.Make;

public static class LFMakeSystem
{
    private static JToken? s_CoreTools;

    private static JToken CoreTools
    {
        get
        {
            if (s_CoreTools != null)
            {
                return s_CoreTools;
            }

            string configsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "configs");
            Directory.CreateDirectory(configsDir);
            string path = Path.Combine(configsDir, "coretools.json");
            if (!File.Exists(path))
            {
                WriteDefaultCoreTools();
            }

            s_CoreTools = JToken.Parse(File.ReadAllText(path));

            return s_CoreTools;
        }
    }

    public static event Func<string[], string>? OnTargetSelect;

    public static void WriteDefaultCoreTools()
    {
        JObject obj = new JObject();

        JObject coreTools = new JObject();
        coreTools["BLC"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blc.exe");
        coreTools["BL"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bl.exe");


        obj["CoreTools"] = coreTools;
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "configs", "coretools.json"), obj.ToString());
    }

    private static JToken? ResolvePath(string path, JToken root)
    {
        string[] parts = path.Split('.');
        JToken? current = root;
        foreach (string part in parts)
        {
            if (current is JObject obj)
            {
                current = obj[part];
            }
            else if (current is JArray arr)
            {
                current = arr[int.Parse(part)];
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private static JToken? ResolvePath(string path, JToken[] roots)
    {
        foreach (JToken jToken in roots)
        {
            JToken? result = ResolvePath(path, jToken);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static string ResolveVariables(string workingDir, string input, params JToken[] roots)
    {
        int start = input.IndexOf("$(", StringComparison.Ordinal);
        while (start != -1)
        {
            int end = input.IndexOf(')', start);
            if (end == -1)
            {
                throw new Exception("Unclosed variable");
            }

            string variable = input.Substring(start + 2, end - start - 2);
            bool isPath = false;
            if (variable.StartsWith('#'))
            {
                isPath = true;
                variable = variable.Substring(1);
            }

            JToken? result = ResolvePath(variable, roots);
            if (result == null)
            {
                throw new Exception($"Variable not found: {variable}");
            }

            string resultValue = result.ToString();

            if (isPath)
            {
                resultValue = Path.GetFullPath(Path.Combine(workingDir, resultValue));
            }

            input = input.Substring(0, start) + resultValue + input.Substring(end + 1);
            start = input.IndexOf("$(", StringComparison.Ordinal);
        }

        return input;
    }

    private static void ProcessDependency(LFCompilerArguments compilerArgs, JToken root, JToken dependency, string workingDir)
    {
        //Check if another project file is specified
        JToken? projectPath = ResolvePath("Project", dependency);
        JToken? target = ResolvePath("Target", dependency);
        JToken? output = ResolvePath("Output", dependency);

        string? targetName = null;
        if (target != null)
        {
            string targetValue = ResolveVariables(workingDir, target.Value<string>(), dependency, root, CoreTools);
            string[] targetParts = targetValue.Split('|');
            if (targetParts.Length == 1)
            {
                targetName = targetParts[0];
            }
            else
            {
                if (OnTargetSelect == null)
                {
                    throw new Exception("Target selector not set");
                }

                targetName = OnTargetSelect?.Invoke(targetParts);
            }
        }

        string? outPath;
        if (projectPath != null)
        {
            string projectFile = Path.Combine(workingDir, ResolveVariables(workingDir, projectPath.Value<string>(), dependency, root, CoreTools));
            outPath = Make(compilerArgs, projectFile, targetName);
        }
        else
        {
            outPath = Make(compilerArgs, root, workingDir, targetName);
        }

        if (output != null)
        {
            string outFile = Path.GetFullPath(Path.Combine(workingDir, ResolveVariables(workingDir, output.Value<string>(), dependency, root, CoreTools)));
            if (outPath == null)
            {
                throw new Exception($"Dependency has no output, can not copy file to {outFile}");
            }

            string outDir = Path.GetDirectoryName(outFile)!;
            Directory.CreateDirectory(outDir);
            Logger.Info($"Copying {outPath} to {outFile}");
            File.Copy(outPath, outFile, true);
        }
    }

    private static void ProcessDependencies(LFCompilerArguments compilerArgs, JToken root, JToken dependencies, string workingDir)
    {
        if (dependencies is JArray deps)
        {
            foreach (JToken dep in deps)
            {
                ProcessDependency(compilerArgs, root, dep, workingDir);
            }
        }
        else
        {
            ProcessDependency(compilerArgs, root, dependencies, workingDir);
        }
    }

    private static void RunCommand(string workingDir, JToken command, params JToken[] roots)
    {
        List<JToken> rootsList = new List<JToken>(roots);
        rootsList.Add(CoreTools);
        string commandValue = ResolveVariables(workingDir, command.Value<string>(), rootsList.ToArray());

        string[] commandParts = commandValue.Split(' ');

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = commandParts[0];
        startInfo.Arguments = string.Join(" ", commandParts.Skip(1));
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        Process proc = new Process();
        proc.StartInfo = startInfo;
        proc.EnableRaisingEvents = true;
        TagLogger logger = new TagLogger(new LFLogger(), "LF");
        proc.Exited += (sender, e) =>
        {
            if (proc.ExitCode != 0)
            {
                logger.Error($"Command {commandValue} failed with exit code {proc.ExitCode}");
            }
        };
        proc.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                logger.Error(e.Data);
            }
        };
        proc.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                logger.Info(e.Data);
            }
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        proc.WaitForExit();

        // string stdOut = string.Join("\n\t", proc.StandardOutput.ReadToEnd().Split('\n'));
        // Logger.Info($"Standard Output of Command: {commandValue}\n\t{stdOut}");
        // string stdErr = string.Join("\n\t", proc.StandardError.ReadToEnd().Split('\n'));
        // if (!string.IsNullOrEmpty(stdErr))
        // {
        //     Logger.Error($"Standard Output of Command: {commandValue}\n\t{stdErr}");
        // }
        //
        //
        // if (proc.ExitCode != 0)
        // {
        //     throw new Exception($"Command failed: {commandValue}");
        // }
    }

    private static void RunCommands(string workingDir, JToken commands, params JToken[] roots)
    {
        if (commands is JArray arr)
        {
            foreach (JToken token in arr)
            {
                RunCommand(workingDir, token, roots);
            }
        }
        else
        {
            RunCommand(workingDir, commands, roots);
        }
    }

    private static void DeleteFile(string workingDir, JToken file, JToken target, JToken root)
    {
        string path = ResolveVariables(workingDir, file.Value<string>(), target, root, CoreTools);
        if (File.Exists(path))
        {
            Logger.Info($"Deleting file: {path}");
            File.Delete(path);
        }
        else
        {
            Logger.Info($"Skip Deleting File: {path} (File does not exist)");
        }
    }

    private static void DeleteFiles(string workingDir, JToken token, JToken target, JToken root)
    {
        if (token is JArray arr)
        {
            foreach (JToken jToken in arr)
            {
                DeleteFile(workingDir, jToken, target, root);
            }
        }
        else
        {
            DeleteFile(workingDir, token, target, root);
        }
    }

    private static string? Make(LFCompilerArguments compilerArgs, JToken root, string workingDirectory, string? targetName = null)
    {
        if (targetName == null)
        {
            targetName = ResolveVariables(workingDirectory, ResolvePath("DefaultTarget", root).Value<string>(), root, CoreTools);
        }

        JToken? target = ResolvePath($"Targets.{targetName}", root);

        if (target == null)
        {
            throw new Exception($"Target not found: {targetName}");
        }

        //Process Dependencies

        Logger.Info($"Processing Target {targetName}");

        JToken? preBuild = ResolvePath("PreBuild", target);

        if (preBuild != null)
        {
            Logger.Info("Running PreBuild Commands");
            RunCommands(workingDirectory, preBuild, target, root);
        }

        JToken? dependencies = ResolvePath("DependsOn", target);
        if (dependencies != null)
        {
            Logger.Info("Processing Dependencies");
            ProcessDependencies(compilerArgs, root, dependencies, workingDirectory);
        }

        string? inputFile = ResolvePath("Input", target)?.Value<string>();
        string? outFile = ResolvePath("Output", target)?.Value<string>();

        string? returnPath = null;
        if (inputFile != null && outFile != null)
        {
            inputFile = Path.GetFullPath(Path.Combine(workingDirectory, ResolveVariables(workingDirectory, inputFile, target, root, CoreTools)));
            outFile = Path.GetFullPath(Path.Combine(workingDirectory, ResolveVariables(workingDirectory, outFile, target, root, CoreTools)));

            Logger.Info($"Making {inputFile} => {outFile}");
            Program.Compile(compilerArgs, inputFile, outFile);
            returnPath = outFile;
        }

        JToken? postBuild = ResolvePath("PostBuild", target);
        if (postBuild != null)
        {
            Logger.Info("Running PostBuild Commands");
            RunCommands(workingDirectory, postBuild, target, root);
        }

        JToken? delete = ResolvePath("Delete", target);
        if (delete != null)
        {
            DeleteFiles(workingDirectory, delete, target, root);
        }

        return returnPath;
    }

    public static string? Make(LFCompilerArguments compilerArgs, string file, string? targetName = null)
    {
        string workingDirectory = Path.GetDirectoryName(Path.GetFullPath(file))!;

        Logger.Debug($"Working Directory: {workingDirectory}");
        Logger.Debug($"Make File: {file}");
        JToken root = JToken.Parse(File.ReadAllText(file));

        return Make(compilerArgs, root, workingDirectory, targetName);
    }
}