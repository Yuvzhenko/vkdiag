using System.Text;
using System.Text.Json;

namespace VkDiag;

/// <summary>
/// Політика іменування JSON-властивостей для конвертації у "snake_case".
/// </summary>
/// <remarks>
/// Використовується при серіалізації/десеріалізації для сумісності зі стандартами Vulkan JSON-маніфестів,
/// де ключі зазвичай записуються як <c>layer_path</c> або <c>api_version</c>, а не <c>LayerPath</c>.
/// </remarks>
public class SnakeCasePolicy : JsonNamingPolicy
{
    /// <summary>
    /// Конвертує вказане ім'я властивості у формат snake_case.
    /// </summary>
    /// <param name="name">Оригінальне ім'я властивості (зазвичай у PascalCase).</param>
    /// <returns>
    /// Ім'я властивості у нижньому регістрі з підкресленнями між словами.
    /// <br/>
    /// Приклад: <c>ApiVersion</c> -> <c>api_version</c>.
    /// </returns>
    /// <remarks>
    /// Алгоритм проходить по кожному символу:
    /// <list type="bullet">
    /// <item>Якщо символ у нижньому регістрі — додає його без змін.</item>
    /// <item>Якщо символ у верхньому регістрі:
    /// <list type="bullet">
    /// <item>Якщо це початок слова (i > 0) — додає підкреслення <c>_</c> перед символом.</item>
    /// <item>Конвертує символ у нижній регістр.</item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    public override string ConvertName(string name)
    {
            var result = new StringBuilder(name.Length + 3);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsLower(c))
                    result.Append(c);
                else
                {
                    c = char.ToLower(c);
                    if (i > 0)
                        result.Append('_').Append(c);
                    else
                        result.Append(c);
                }
            }
            return result.ToString();
        }
}