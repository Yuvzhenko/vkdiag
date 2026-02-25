namespace VkDiag;

/// <summary>
/// Перелік можливих статусів підтримки операційної системи.
/// </summary>
/// <remarks>
/// Використовується для визначення, чи є поточна версія Windows стабільною, 
/// застарілою або тестовою (Insider Preview).
/// </remarks>
public enum OsSupportStatus
{
    /// <summary>
    /// Статус невідомий або не вдалося розпізнати версію ОС.
    /// </summary>
    Unknown,

    /// <summary>
    /// Операційна система повністю підтримується (актуальний стабільний реліз).
    /// </summary>
    Supported,

    /// <summary>
    /// Попередня версія ОС (Insider Preview, Beta, Dev, Canary канали).
    /// </summary>
    /// <remarks>
    /// Ця версія може бути нестабільною та містити помилки, що впливають на Vulkan.
    /// </remarks>
    Prerelease,

    /// <summary>
    /// Застаріла версія ОС, офіційна підтримка якої припинена (End of Life).
    /// </summary>
    /// <remarks>
    /// Наприклад, Windows 7, 8.1 або старі збірки Windows 10/11, які більше не отримують оновлень.
    /// </remarks>
    Deprecated,
}