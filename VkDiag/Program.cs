using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using Mono.Options;
using VkDiag.POCOs;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("vkdiag.Test")]

namespace VkDiag;

internal static partial class Program
{
    
    private static bool isAdmin;
    private static bool autofix;
    private static bool clear;
    private static bool disableLayers;
    private static bool ignoreHighPerfCheck;
    internal static bool everythingIsFine = true;
    private static bool hasBrokenEntries;
    private static bool hasProperVulkanDrivers;
    private static bool hasExplicitDriverReg;
    private static bool hasConflictingLayers;
    private static bool disabledConflictingLayers = true;
    private static bool removedExplicitDriverReg = true;
    private static bool fixedEverything = true;
    public static Action<ProcessStartInfo> ProcessStarter = psi => Process.Start(psi);
    public static Action<int> Exiter = code => Environment.Exit(code);

    public static async Task Main(string[] args)
    {
        try
        {
            using var mutex = new System.Threading.Mutex(false, Constants.mutexPath);

            if (!mutex.WaitOne(0, false))
            {
                LogError("VkDiag is already running!");
                Console.ReadKey();
                return;
            }

            CheckPermissions();
            GetOptions(args);
                
            try
            {
                Console.Title = "Vulkan Diagnostics Tool v" + Constants.VkDiagVersion;
                Console.WindowWidth = Math.Min(Console.LargestWindowWidth, 100);
                Console.WindowHeight = Math.Min(Console.LargestWindowHeight, 60);
                Console.BufferWidth = Console.WindowWidth;
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch {}
                
            if (!Environment.Is64BitOperatingSystem)
            {
                LogError("Only 64-bit OS is supported");
                Environment.Exit(-1);
            }

            await CheckVkDiagVersionAsync().ConfigureAwait(false);
            var osVer = CheckOs();
            var windowsService = new WindowsPackageService();
            if (osVer.Major >= 10)
                try { CheckAppxPackages(windowsService); } catch { }

            var (hasInactiveGpus, hasVulkanGpus) = CheckGpuDrivers();
            if (!hasVulkanGpus)
            {
                everythingIsFine = false;
                LogError("No GPUs registered with Vulkan support");
            }
            if (hasInactiveGpus && osVer.Major >= 10)
            {
                WriteLogLine();
                WriteLogLine("User GPU Preferences:");
                try
                {
                    if (!HasPerformanceModeProfile())
                    {
                        LogWarning("Running without High performance GPU profile");
                        if (!ignoreHighPerfCheck)
                            Restart(false, false);
                    }
                    else
                        LogSuccess("Running with High performance GPU profile");
                }
                catch
                {
                    LogWarning("Failed to set High performance GPU profile");
                }
            }
            CheckVulkanMeta();

            ShowMenu();
        }
        catch (Exception e)
        {
            Console.WriteLine();
            LogError("CRITICAL ERROR!\n" + e.ToString());
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    private static async Task CheckVkDiagVersionAsync()
    {
        try
        {
            using var client = new HttpClient();
            var curVerParts = Constants.VkDiagVersion.Split([' ', '-'], 2);
            client.DefaultRequestHeaders.UserAgent.Add(new("vkdiag", curVerParts[0]));
            var responseJson = await client.GetStringAsync(Constants.releases).ConfigureAwait(false);
            var releaseList = JsonSerializer.Deserialize<List<GitHubReleaseInfo>>(responseJson, Constants.JsonOptions);
            releaseList = releaseList?.OrderByDescending(r => Version.TryParse(r.TagName.TrimStart('v'), out var v) ? v : null).ToList();
            var latest = releaseList?.FirstOrDefault(r => !r.Prerelease);
            var latestBeta = releaseList?.FirstOrDefault(r => r.Prerelease);
            Version.TryParse(curVerParts[0], out var curVer);
            Version.TryParse(latest?.TagName.TrimStart('v') ?? "0", out var latestVer);
            var latestBetaParts = latestBeta?.TagName.Split([' ', '-'], 2);
            Version.TryParse(latestBetaParts?[0] ?? "0", out var latestBetaVer);
            if (latestVer > curVer || latestVer == curVer && curVerParts.Length > 1)
            {
                LogWarning("VkDiag version: " + Constants.VkDiagVersion);
                LogWarning($"    Newer version available: {latestVer}");
            }
            else
                LogSuccess("VkDiag version: " + Constants.VkDiagVersion);
            if (latestBetaVer > latestVer
                || (latestVer == latestBetaVer
                    && curVerParts.Length > 1
                    && (latestBetaParts?.Length > 1 && latestBetaParts[1] != curVerParts[1]
                        || (latestBetaParts?.Length ?? 0) == 0)))
                WriteLogLine(DefaultFgColor, "+", $"    Newer prerelease version available: {latestBetaVer}");
        }
        catch
        {
            WriteLogLine(DefaultFgColor, "+", "VkDiag version: " + Constants.VkDiagVersion);
            LogWarning($"    Failed to check for updates");
        }
    }

    private static void GetOptions(string[] args)
    {
        var help = false;
        var options = new OptionSet
        {
            {"?|h|help", _ => help = true},
            {"i|ignore-high-performance-check", _ => ignoreHighPerfCheck = true},
            {"f|fix", "Remove broken Vulkan entries", _ => autofix = true},
            {"c|clear-explicit-driver-reg", "Remove explicit Vulkan driver registration", _ => clear = true},
            {"d|disable-incompatible-layers", "Disable potentially incompatible implicit Vulkan layers", _ => disableLayers = true}
        };
        options.Parse(args);

        if (help)
        {
            WriteLogLine("RPCS3 Vulkan diagnostics tool");
            WriteLogLine("Usage:");
            WriteLogLine("  vkdiag [OPTIONS]");
            WriteLogLine("Available options:");
            lock (TheDoor) options.WriteOptionDescriptions(Console.Out);
            Environment.Exit(0);
        }
    }

    private static void CheckPermissions()
        => isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private static void RestartIfNotElevated() => Restart();
        
    private static void Restart(bool onlyToElevate = true, bool requireElevation = true)
    {
        if (isAdmin && onlyToElevate)
            return;
            
        if (requireElevation)
            WriteLogLine("Restarting with elevated permissions...");
        else
            WriteLogLine("Restarting...");
        var args = "";
        if (autofix)
            args += " -f";
        if (clear)
            args += " -c";
        if (disableLayers)
            args += " -d";
        args = args.TrimStart();
        var cmd = Environment.GetCommandLineArgs()[0];
        var wtProfile = Environment.GetEnvironmentVariable("WT_PROFILE_ID");
        if (!string.IsNullOrEmpty(wtProfile))
        {
            args = $"\"{cmd}\" {args}";
            cmd = "wt";
        }
        var psi = new ProcessStartInfo
        {
            Verb = requireElevation ? "runas" : "open",
            UseShellExecute = true,
            FileName = cmd,
            Arguments = args,
        };
        ProcessStarter(psi);
        Exiter(0);
    }

    private static void ShowMenu()
    {
        bool restartNeeded = false;
        bool anyIssuesFound = false;

        if (hasBrokenEntries && !fixedEverything)
        {
            anyIssuesFound = true;
            LogWarning("Found broken Vulkan entries.");
            if (AskUserYesNo("Do you want to remove them?"))
            {
                autofix = true;
                restartNeeded = true;
            }
        }

        if (hasConflictingLayers && !disabledConflictingLayers)
        {
            anyIssuesFound = true;
            LogWarning("Found incompatible Vulkan layers.");
            if (AskUserYesNo("Do you want to disable them?"))
            {
                disableLayers = true;
                restartNeeded = true;
            }
        }

        if (hasExplicitDriverReg && hasProperVulkanDrivers)
        {
            anyIssuesFound = true;
            LogWarning("Found legacy explicit Vulkan driver registration.");
            if (AskUserYesNo("Do you want to clear it?"))
            {
                removedExplicitDriverReg = true;
                restartNeeded = true;
            }
        }

        if (restartNeeded)
        {
            Restart(false);
            Environment.Exit(0); 
        }
        
        Console.WriteLine();
        if (!anyIssuesFound && everythingIsFine)
        {
            LogSuccess("Everything seems to be fine.");
        }
        else
        {
            LogInfo("Diagnostics finished. No changes were made.");
        }

        Console.WriteLine();
        Console.WriteLine("Remember to screenshot or copy this screen content for support.");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit the tool...");
        Console.ReadKey();
        Environment.Exit(0);
    }

    private static bool AskUserYesNo(string question)
    {
        Console.Write($"{question} [Y/N]: ");
        while (true)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Y)
            {
                Console.WriteLine("Y");
                return true;
            }
            if (key == ConsoleKey.N || key == ConsoleKey.Escape)
            {
                Console.WriteLine("N");
                return false;
            }
        }
    }
}