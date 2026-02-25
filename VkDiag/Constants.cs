using System;
using System.Collections.Generic;
using System.Text.Json;

namespace VkDiag;

/// <summary>
/// Містить глобальні константи, шляхи до реєстру та конфігураційні списки для програми.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Поточна версія утиліти VkDiag.
    /// </summary>
    internal static readonly string VkDiagVersion = "1.3.13";

    /// <summary>
    /// Список відомих пакетів Windows (Appx/MSIX), які можуть викликати конфлікти з Vulkan.
    /// </summary>
    /// <remarks>
    /// Кортеж містить:
    /// <list type="bullet">
    /// <item><description><c>id</c>: ID сімейства пакетів.</description></item>
    /// <item><description><c>title</c>: Читабельна назва для відображення.</description></item>
    /// </list>
    /// </remarks>
    internal static readonly List<(string id, string title)> KnownPackages =
    [
        ("Microsoft.D3DMappingLayers_8wekyb3d8bbwe", "OpenCL, OpenGL, and Vulkan Compatibility Pack"),
    ];

    /// <summary>
    /// Список служб або назв драйверів, які ідентифікують віртуальні або базові графічні адаптери.
    /// </summary>
    /// <remarks>
    /// Ці пристрої зазвичай ігноруються при перевірці драйверів Vulkan, оскільки вони не є фізичними GPU.
    /// </remarks>
    internal static readonly HashSet<string> ServiceBlockList =
    [
        "BasicDisplay",
        "WUDFRd",
        "HyperVideo",
        "MS Idd Device",
        "IndirectKmd",
        "spacedesk Graphics Adapter",

        "LuminonCore IDDCX Adapter",
        "Parsec Virtual Display Adapter"
    ];
    
    /// <summary>
    /// Словник відомих проблемних неявних (implicit) шарів Vulkan.
    /// </summary>
    /// <remarks>
    /// Структура словника:
    /// <list type="bullet">
    /// <item><description><b>Key</b>: Ім'я JSON-файлу шару.</description></item>
    /// <item><description><b>Value</b>: Мінімальна безпечна версія. Якщо <c>null</c>, шар вважається проблемним у будь-якій версії.</description></item>
    /// </list>
    /// Використовується для виявлення конфліктного ПЗ (наприклад, оверлеїв OBS, Bandicam, Action!).
    /// </remarks>
    internal static readonly Dictionary<string, Version> KnownProblematicLayers = new()
    {
        ["MirillisActionVulkanLayer.json"] = null,
        ["ow-vulkan-overlay64.json"] = null,
        ["ow-graphics-vulkan64.json"] = null,
        ["fpsmonvk64.json"] = null,
        ["fpsmonvk32.json"] = null,
        ["hudsightvk64.json"] = null,
        ["hudsightvk32.json"] = null,
        ["playclawvk64.json"] = null,
        ["playclawvk32.json"] = null,
        ["VK_LAYER_FCAT_DT_overlay_JSON_x64.json"] = null,
        ["VK_LAYER_FCAT_DT_overlay_JSON_x86.json"] = null,
        ["bdcamvk64.json"] = new(1, 1, 0, 111),
        ["bdcamvk32.json"] = new(1, 1, 0, 111),
        ["obs-vulkan64.json"] = new(1, 2, 2, 0),
        ["obs-vulkan32.json"] = new(1, 2, 2, 0),
    };

    /// <summary>
    /// URL-адреса GitHub API для перевірки наявності нових релізів.
    /// </summary>
    internal const string releases = "https://api.github.com/repos/13xforever/vkdiag/releases";

    /// <summary>
    /// Шлях до реєстру Windows, де зберігаються налаштування продуктивності GPU для окремих програм.
    /// </summary>
    internal const string UserGpuPreferencesPath = @"Software\Microsoft\DirectX\UserGpuPreferences";

    /// <summary>
    /// Глобальні налаштування серіалізації JSON.
    /// </summary>
    /// <remarks>
    /// Використовує <see cref="SnakeCasePolicy"/> для іменування полів та форматований вивід.
    /// </remarks>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = new SnakeCasePolicy(),
        WriteIndented = true,
    };

    /// <summary>
    /// Шлях до гілки реєстру Khronos/Vulkan (для 64-бітних додатків в x64 ОС).
    /// </summary>
    public const string VulkanRegistryPath = @"SOFTWARE\Khronos\Vulkan";

    /// <summary>
    /// Шлях до гілки реєстру Khronos/Vulkan (для 32-бітних додатків в x64 ОС).
    /// </summary>
    public const string VulkanRegistryPathWow64 = @"SOFTWARE\WOW6432Node\Khronos\Vulkan";

    /// <summary>
    /// Унікальне ім'я глобального м'ютексу для запобігання запуску кількох екземплярів програми.
    /// </summary>
    internal const string mutexPath = "Global\\VkDiag_Tool_Mutex";
}