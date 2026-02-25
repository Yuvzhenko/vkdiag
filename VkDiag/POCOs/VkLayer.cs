using System.Collections.Generic;

namespace VkDiag.POCOs;

/// <summary>
/// Представляє об'єкт шару (Layer) з JSON-маніфесту Vulkan.
/// </summary>
/// <remarks>
/// Цей клас використовується для десеріалізації масиву <c>layers</c> або кореневого об'єкта <c>layer</c> 
/// у файлах конфігурації Vulkan (наприклад, <c>VK_LAYER_KHRONOS_validation.json</c>).
/// <para>
/// Властивості цього класу автоматично заповнюються з ключів JSON у форматі snake_case 
/// (наприклад, <c>library_path</c> -> <c>LibraryPath</c>).
/// </para>
/// Див. офіційну документацію: <see href="https://github.com/KhronosGroup/Vulkan-Loader/blob/master/docs/LoaderLayerInterface.md#layer-manifest-file-format">Layer Manifest File Format</see>.
/// </remarks>
public class VkLayer
{
    /// <summary>
    /// Унікальне ім'я шару (наприклад, "VK_LAYER_KHRONOS_validation").
    /// </summary>
    /// <value>Рядок, що ідентифікує шар.</value>
    public string Name { get; set; }

    /// <summary>
    /// Тип шару.
    /// </summary>
    /// <remarks>
    /// Зазвичай приймає значення "INSTANCE", "DEVICE" або "GLOBAL".
    /// Сучасні шари переважно є "INSTANCE".
    /// </remarks>
    public string Type { get; set; }

    /// <summary>
    /// Шлях до бінарного файлу бібліотеки (.dll або .so), що реалізує шар.
    /// </summary>
    /// <remarks>
    /// Може бути повним абсолютним шляхом або відносним (відносно розташування JSON-файлу).
    /// </remarks>
    public string LibraryPath { get; set; }

    /// <summary>
    /// Версія Vulkan API, яку підтримує цей шар.
    /// </summary>
    /// <remarks>
    /// Зазвичай рядок у форматі "1.x.y", який пізніше парситься у <see cref="System.Version"/>.
    /// </remarks>
    public string ApiVersion { get; set; } // Version

    /// <summary>
    /// Версія реалізації самого шару.
    /// </summary>
    /// <remarks>
    /// Це число або рядок, що вказує на версію білда шару (не плутати з версією API).
    /// </remarks>
    public string ImplementationVersion { get; set; }

    /// <summary>
    /// Короткий опис призначення шару для відображення користувачу.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Словник змінних середовища, наявність яких увімкне цей шар.
    /// </summary>
    /// <remarks>
    /// Використовується для неявних (implicit) шарів. Якщо змінна середовища задана, шар активується автоматично.
    /// </remarks>
    public Dictionary<string, string> EnableEnvironment { get; set; }

    /// <summary>
    /// Словник змінних середовища, наявність яких вимкне цей шар.
    /// </summary>
    /// <remarks>
    /// Дозволяє користувачеві примусово вимкнути неявний шар, встановивши відповідну змінну.
    /// </remarks>
    public Dictionary<string, string> DisableEnvironment { get; set; }
}