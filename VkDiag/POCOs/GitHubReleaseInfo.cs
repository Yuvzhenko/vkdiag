using System;

namespace VkDiag.POCOs;

/// <summary>
/// Модель даних, що представляє інформацію про реліз на GitHub.
/// </summary>
/// <remarks>
/// Використовується для десеріалізації JSON-відповіді від GitHub API.
/// Властивості цього класу автоматично мапляться на snake_case ключі JSON (наприклад, <c>TagName</c> -> <c>tag_name</c>)
/// завдяки політиці <see cref="VkDiag.SnakeCasePolicy"/>.
/// </remarks>
public class GitHubReleaseInfo
{
    /// <summary>
    /// URL-адреса API для цього релізу.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// URL-адреса API для отримання списку ассетів (файлів) релізу.
    /// </summary>
    public string AssetsUrl { get; set; }

    /// <summary>
    /// Публічна HTML-сторінка релізу (для перегляду у браузері).
    /// </summary>
    public string HtmlUrl { get; set; }

    /// <summary>
    /// Унікальний ідентифікатор релізу в системі GitHub.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Назва тегу git, пов'язаного з релізом (наприклад, "v1.3.13").
    /// </summary>
    /// <remarks>
    /// Використовується програмою для порівняння поточної версії з доступною на сервері.
    /// </remarks>
    public string TagName { get; set; }

    /// <summary>
    /// Назва релізу (заголовок).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Опис релізу (body), що зазвичай містить список змін (changelog).
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Вказує, чи є цей реліз попереднім (Alpha/Beta/RC).
    /// </summary>
    /// <value>
    /// <c>true</c>, якщо це пре-реліз; інакше <c>false</c>.
    /// </value>
    public bool Prerelease { get; set; }

    /// <summary>
    /// Дата та час створення релізу.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата та час публікації релізу.
    /// </summary>
    public DateTime PublishedAt { get; set; }
}