using System.Collections.Generic;
using VkDiag;

namespace vkdiag.Test;

public class FakePackageService : IPackageService
{
    private readonly List<string> _foundPackages;

    // Constructor that allows us to define what packages will be "found"
    public FakePackageService(List<string> foundPackages)
    {
        _foundPackages = foundPackages;
    }

    // It returns only what we provided in the constructor
    public IEnumerable<string> FindPackages(string id) => _foundPackages;
    
    public string GetVersion(string name) => "1.0.0.0";
    
    public string GetName(string name, string title) => "Test Package";
}