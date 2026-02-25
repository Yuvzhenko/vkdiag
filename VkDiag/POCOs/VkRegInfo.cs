using System.Collections.Generic;

namespace VkDiag.POCOs;

/// <summary>
/// Кореневий об'єкт (Root DTO) для десеріалізації JSON-маніфесту шару Vulkan.
/// </summary>
/// <remarks>
/// Цей клас відображає структуру файлу маніфесту (наприклад, <c>VkLayer_*.json</c>).
/// Властивості автоматично мапляться зі snake_case ключів JSON завдяки <see cref="VkDiag.SnakeCasePolicy"/>.
/// <para>
/// Див. специфікацію: <see href="https://github.com/KhronosGroup/Vulkan-Loader/blob/master/docs/LoaderLayerInterface.md#layer-manifest-file-format">Layer Manifest File Format</see>.
/// </para>
/// </remarks>
public class VkRegInfo
{
    /// <summary>
    /// Версія формату файлу маніфесту (наприклад, "1.0.0", "1.1.2" або "1.2.0").
    /// </summary>
    /// <remarks>
    /// Відповідає ключу JSON <c>file_format_version</c>.
    /// Це поле визначає обов'язкові поля та структуру документа.
    /// </remarks>
    public string FileFormatVersion { get; set; } // Version

    /// <summary>
    /// Список шарів, визначених у маніфесті (для форматів, що підтримують множинні шари).
    /// </summary>
    /// <remarks>
    /// Відповідає ключу JSON <c>layers</c>.
    /// Хоча сучасна специфікація надає перевагу одиничному об'єкту <c>layer</c>,
    /// програма підтримує і старий формат зі списком для сумісності.
    /// </remarks>
    public List<VkLayer> Layers { get; set; }

    /// <summary>
    /// Об'єкт шару, визначений у маніфесті.
    /// </summary>
    /// <remarks>
    /// Відповідає ключу JSON <c>layer</c>.
    /// Це стандартний спосіб опису шару в сучасних маніфестах (починаючи з версії формату 1.1.0).
    /// </remarks>
    public VkLayer Layer { get; set; }
}