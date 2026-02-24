using System;
using System.Collections.Generic;
using VkDiag.Interop;

namespace VkDiag;

internal static partial class Program
{ 
    private static void CheckAppxPackages(IPackageService packageService)
    {
        var found = new List<(string name, string version)>();
        foreach (var pkg in Constants.KnownPackages)
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
            LogWarning("Potentially incompatible software:");
            foreach (var pkg in found)
                LogWarning($"    {pkg.name}{pkg.version}");
        }
    }
    public class WindowsPackageService : IPackageService
    {
        public IEnumerable<string> FindPackages(string id) => PackageManager.FindPackagesByPackageFamily(id);
        public string GetVersion(string name) => PackageManager.GetPackageVersion(name, "");
        public string GetName(string name, string title) => PackageManager.GetAppStoreName(name, title);
    }
}