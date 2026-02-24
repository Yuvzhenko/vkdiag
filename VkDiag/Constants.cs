using System;
using System.Collections.Generic;
using System.Text.Json;

namespace VkDiag;

internal static class Constants
{
    internal static readonly string VkDiagVersion = "1.3.13";
    internal static readonly List<(string id, string title)> KnownPackages =
    [
        ("Microsoft.D3DMappingLayers_8wekyb3d8bbwe", "OpenCL, OpenGL, and Vulkan Compatibility Pack"),
    ];
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
    internal const string releases = "https://api.github.com/repos/13xforever/vkdiag/releases";
    internal const string UserGpuPreferencesPath = @"Software\Microsoft\DirectX\UserGpuPreferences";
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = new SnakeCasePolicy(),
        WriteIndented = true,
    };
    public const string VulkanRegistryPath = @"SOFTWARE\Khronos\Vulkan";
    public const string VulkanRegistryPathWow64 = @"SOFTWARE\WOW6432Node\Khronos\Vulkan";

    internal const string mutexPath = "Global\\VkDiag_Tool_Mutex";
}