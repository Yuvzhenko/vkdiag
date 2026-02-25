using System;

namespace VkDiag;

internal static partial class Program
{
    /// <summary>
    /// Зберігає початковий колір тексту консолі для його відновлення після кольорового виводу.
    /// </summary>
    private static readonly ConsoleColor DefaultFgColor = Console.ForegroundColor;

    /// <summary>
    /// Об'єкт синхронізації (lock object) для забезпечення потокобезпечного виводу в консоль.
    /// </summary>
    /// <remarks>
    /// Запобігає "перемішуванню" символів при одночасному записі з різних потоків 
    /// (хоча в поточному коді програма переважно однопотокова, це гарна практика).
    /// </remarks>
    private static readonly object TheDoor = new();
    
    /// <summary>
    /// Виводить форматоване повідомлення зі статусом у квадратних дужках.
    /// </summary>
    /// <param name="statusColor">Колір, яким буде виведено символ статусу.</param>
    /// <param name="status">Короткий символ статусу (наприклад, "+", "x", "!").</param>
    /// <param name="description">Текст повідомлення.</param>
    /// <remarks>
    /// Метод автоматично визначає відступ (пробіли на початку <paramref name="description"/>) 
    /// і вставляє маркер статусу <b>після</b> відступу, щоб зберегти ієрархію логування.
    /// <br/>
    /// Формат виводу: <c>[відступ][статус] повідомлення</c>
    /// </remarks>
    private static void WriteLogLine(ConsoleColor statusColor, string status, string description)
    {
        var val = description.TrimStart();
        var prefix = "";
        if (val.Length < description.Length)
            prefix = description.Substring(0, description.Length - val.Length);

        lock (TheDoor)
        {
            Console.Write(prefix + '[');
            Console.ForegroundColor = statusColor;
            Console.Write(status);
            Console.ForegroundColor = DefaultFgColor;
            Console.WriteLine("] " + val);
        }
    }
    
    /// <summary>
    /// Виводить рядок тексту заданим кольором.
    /// </summary>
    /// <param name="statusColor">Колір тексту.</param>
    /// <param name="line">Текст для виводу.</param>
    /// <remarks>
    /// Автоматично повертає колір консолі до <see cref="DefaultFgColor"/> після завершення виводу.
    /// </remarks>
    private static void WriteLogLine(ConsoleColor statusColor, string line)
    {
        lock (TheDoor)
        {
            Console.ForegroundColor = statusColor;
            Console.WriteLine(line);
            Console.ForegroundColor = DefaultFgColor;
        }
    }

    /// <summary>
    /// Потокобезпечна обгортка для стандартного <see cref="Console.WriteLine(string)"/>.
    /// </summary>
    /// <param name="line">Текст для виводу.</param>
    private static void WriteLogLine(string line)
    {
        lock (TheDoor) Console.WriteLine(line);
    }

    /// <summary>
    /// Виводить порожній рядок для візуального розділення блоків тексту.
    /// </summary>
    /// <remarks>
    /// Використовує символ нульової ширини (<c>\u200b</c>), щоб обійти специфічну поведінку консолі,
    /// коли порожній <c>WriteLine</c> може видавати лише <c>\r</c> замість <c>\r\n</c> у деяких терміналах.
    /// </remarks>
    private static void WriteLogLine()
    {
        lock (TheDoor) Console.WriteLine('\u200b'); // zero width space to workaround bug with emitted \r instead of \r\n
    }

    /// <summary>
    /// Логує повідомлення про помилку (червоний хрестик [x]).
    /// </summary>
    /// <param name="message">Текст помилки.</param>
    private static void LogError(string message) 
        => WriteLogLine(ConsoleColor.Red, "x", message);

    /// <summary>
    /// Логує попередження (темно-жовтий знак оклику [!]).
    /// </summary>
    /// <param name="message">Текст попередження.</param>
    private static void LogWarning(string message) 
        => WriteLogLine(ConsoleColor.DarkYellow, "!", message);

    /// <summary>
    /// Логує повідомлення про успішну операцію (зелений плюс [+]).
    /// </summary>
    /// <param name="message">Текст повідомлення.</param>
    private static void LogSuccess(string message) 
        => WriteLogLine(ConsoleColor.Green, "+", message);

    /// <summary>
    /// Логує інформаційне повідомлення (блакитна літера [i]).
    /// </summary>
    /// <param name="message">Текст інформації.</param>
    private static void LogInfo(string message) 
        => WriteLogLine(ConsoleColor.Cyan, "i", message);
}