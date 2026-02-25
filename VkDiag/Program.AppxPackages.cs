using System;
using System.Collections.Generic;
using VkDiag.Interop;

namespace VkDiag;

internal static partial class Program
{ 
    /// <summary>
    /// Перевіряє наявність встановлених Appx-пакетів (MSIX), які відомі своєю несумісністю з Vulkan.
    /// </summary>
    /// <remarks>
    /// Метод перебирає список <see cref="Constants.KnownPackages"/>. Якщо знайдено проблемний пакет:
    /// <list type="number">
    /// <item>Встановлюється глобальний прапорець <see cref="everythingIsFine"/> у <c>false</c>.</item>
    /// <item>У лог виводиться попередження з назвою та версією пакета.</item>
    /// </list>
    /// Будь-які виключення під час пошуку пакетів ігноруються (try-catch без обробки), 
    /// щоб не переривати роботу утиліти через помилки доступу до Windows Store API.
    /// </remarks>
    /// <param name="packageService">
    /// Інтерфейс сервісу для отримання інформації про пакети (зазвичай <see cref="WindowsPackageService"/>).
    /// </param>
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

    /// <summary>
    /// Реалізація сервісу пакетів для середовища Windows.
    /// </summary>
    /// <remarks>
    /// Цей клас виступає обгорткою над статичним класом <see cref="VkDiag.Interop.PackageManager"/>,
    /// забезпечуючи можливість підміни (mocking) у тестах через інтерфейс <see cref="IPackageService"/>.
    /// </remarks>
    public class WindowsPackageService : IPackageService
    {
        /// <inheritdoc />
        public IEnumerable<string> FindPackages(string id) => PackageManager.FindPackagesByPackageFamily(id);

        /// <inheritdoc />
        public string GetVersion(string name) => PackageManager.GetPackageVersion(name, "");

        /// <inheritdoc />
        public string GetName(string name, string title) => PackageManager.GetAppStoreName(name, title);
    }
}