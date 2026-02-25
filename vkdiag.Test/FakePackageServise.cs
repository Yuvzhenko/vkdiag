using System.Collections.Generic;
using VkDiag;

namespace vkdiag.Test;

/// <summary>
/// Реалізація-заглушка (Mock/Stub) сервісу пакетів для використання в юніт-тестах.
/// </summary>
/// <remarks>
/// Цей клас дозволяє тестувати логіку <see cref="VkDiag.Program.CheckAppxPackages"/> без необхідності
/// мати реальні встановлені пакети Windows Store або викликати системні API.
/// </remarks>
public class FakePackageService : IPackageService
{
    /// <summary>
    /// Внутрішній список пакетів, які будуть "знайдені" під час тесту.
    /// </summary>
    private readonly List<string> _foundPackages;

    // Constructor that allows us to define what packages will be "found"
    /// <summary>
    /// Ініціалізує новий екземпляр заглушки з визначеним набором пакетів.
    /// </summary>
    /// <param name="foundPackages">Список повних імен пакетів, які метод <see cref="FindPackages"/> повинен повернути.</param>
    public FakePackageService(List<string> foundPackages)
    {
        _foundPackages = foundPackages;
    }

    // It returns only what we provided in the constructor
    /// <summary>
    /// Емулює пошук пакетів, повертаючи заздалегідь визначений список.
    /// </summary>
    /// <param name="id">Ідентифікатор сімейства пакетів (ігнорується в цій реалізації).</param>
    /// <returns>Список пакетів, переданий у конструктор.</returns>
    public IEnumerable<string> FindPackages(string id) => _foundPackages;
    
    /// <summary>
    /// Повертає фіксовану версію для будь-якого пакета.
    /// </summary>
    /// <param name="name">Повне ім'я пакета (ігнорується).</param>
    /// <returns>Рядок "1.0.0.0".</returns>
    public string GetVersion(string name) => "1.0.0.0";
    
    /// <summary>
    /// Повертає фіксовану назву для будь-якого пакета.
    /// </summary>
    /// <param name="name">Повне ім'я пакета (ігнорується).</param>
    /// <param name="title">Назва за замовчуванням (ігнорується).</param>
    /// <returns>Рядок "Test Package".</returns>
    public string GetName(string name, string title) => "Test Package";
}