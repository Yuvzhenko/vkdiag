using System.Collections.Generic;

namespace VkDiag;

public interface IPackageService
{
    IEnumerable<string> FindPackages(string packageFamilyID);
    string GetVersion(string fullName);
    string GetName(string fullName, string defaultTitle);
}