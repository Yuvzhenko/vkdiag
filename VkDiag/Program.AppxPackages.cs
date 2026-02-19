using System;
using System.Collections.Generic;
using VkDiag.Interop;

namespace VkDiag;

internal static partial class Program
{
    // ReSharper disable StringLiteralTypo
    private static readonly List<(string id, string title)> KnownPackages =
    [
        ("Microsoft.D3DMappingLayers_8wekyb3d8bbwe", "OpenCL, OpenGL, and Vulkan Compatibility Pack"),
    ];
    // ReSharper restore StringLiteralTypo
    
    internal static void CheckAppxPackages(IPackageService packageService)
    {
        var found = new List<(string name, string version)>();
        foreach (var pkg in KnownPackages)
        {
            try
            {
                var pkgFullNameList = packageService.FindPackages(pkg.id);
                foreach (var pkgFullName in pkgFullNameList)
                {
                    var appStoreName = packageService.GetName(pkgFullName, pkg.title);
                    var ver = packageService.GetVersion(pkgFullName);
                    found.Add((appStoreName, ver));
                }
            }
            catch
            {}
        }
        if (found is { Count: > 0 })
        {
            everythingIsFine = false;
            WriteLogLine();
            WriteLogLine(ConsoleColor.DarkYellow, "!", "Potentially incompatible software:");
            foreach (var pkg in found)
                WriteLogLine(ConsoleColor.DarkYellow, "!", $"    {pkg.name}{pkg.version}");
        }
    }
    public class WindowsPackageService : IPackageService
{
    public IEnumerable<string> FindPackages(string id) => PackageManager.FindPackagesByPackageFamily(id);
    public string GetVersion(string name) => PackageManager.GetPackageVersion(name, "");
    public string GetName(string name, string title) => PackageManager.GetAppStoreName(name, title);
}
}