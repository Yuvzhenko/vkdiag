using System.Collections.Generic;

namespace VkDiag;

/// <summary>
/// Інтерфейс для абстракції взаємодії з системою пакетів Windows (Appx/MSIX).
/// </summary>
/// <remarks>
/// Дозволяє отримувати інформацію про встановлені додатки з Microsoft Store,
/// які можуть впливати на роботу Vulkan (наприклад, пакети сумісності OpenCL/OpenGL).
/// </remarks>
public interface IPackageService
{
    /// <summary>
    /// Шукає встановлені пакети за їх ідентифікатором сімейства (Package Family ID).
    /// </summary>
    /// <param name="packageFamilyID">
    /// Унікальний ідентифікатор сімейства пакетів (PFN без версії та хешу, або схожий ідентифікатор).
    /// </param>
    /// <returns>
    /// Перелік повних імен (Package Full Names) знайдених пакетів.
    /// Повертає порожній перелік, якщо нічого не знайдено.
    /// </returns>
    IEnumerable<string> FindPackages(string packageFamilyID);

    /// <summary>
    /// Отримує версію пакета на основі його повного імені.
    /// </summary>
    /// <param name="fullName">Повне ім'я пакета (Package Full Name).</param>
    /// <returns>
    /// Рядок з версією пакета (наприклад, "1.0.0.0").
    /// Якщо версію не вдалося визначити, повертає порожній рядок.
    /// </returns>
    string GetVersion(string fullName);

    /// <summary>
    /// Отримує читабельну назву додатка (App Name) для відображення користувачеві.
    /// </summary>
    /// <param name="fullName">Повне ім'я пакета (Package Full Name).</param>
    /// <param name="defaultTitle">
    /// Назва за замовчуванням, яка буде повернута, якщо не вдасться отримати справжню назву з маніфесту пакета.
    /// </param>
    /// <returns>
    /// Локалізована назва додатка або <paramref name="defaultTitle"/> у разі помилки.
    /// </returns>
    string GetName(string fullName, string defaultTitle);
}