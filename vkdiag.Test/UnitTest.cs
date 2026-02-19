using Xunit;
using VkDiag;
using System.Reflection;
using System;
using System.IO;

namespace vkdiag.Test;

public class SnakeCasePolicyTests
{
    [Theory]
    [InlineData("LibraryPath", "library_path")]
    [InlineData("VulkanDriverName", "vulkan_driver_name")]
    [InlineData("ApiVersion", "api_version")]
    public void ConvertNameTest(string input, string expected)
    {
        //Set up the environment and objects needed for the test
        var policy = new SnakeCasePolicy();
        // Perform the actual action we want to test
        var result = policy.ConvertName(input);
        // Assert: Verify that the result matches our expectations
        Assert.Equal(expected, result);
    }
}

public class AppxPackagesTests
{
    [Fact]
    public void CheckAppxPackages_PackageIsFound()
    {
        // Arrange: Simulate that one package was found
        var packagesFound = new List<string> { "Fake_Incompatible_Package" };
        var serviceWithPackage = new FakePackageService(packagesFound);
        Program.everythingIsFine = true;

        // Act
        Program.CheckAppxPackages(serviceWithPackage);

        // Assert
        Assert.False(Program.everythingIsFine, "Status should be false because a package was found.");
    }

    [Fact]
    public void CheckAppxPackages_NoPackagesFound()
    {
        // Arrange: Simulate that NO packages were found (empty list)
        var noPackages = new List<string>(); 
        var emptyService = new FakePackageService(noPackages);
        Program.everythingIsFine = true;

        // Act
        Program.CheckAppxPackages(emptyService);

        // Assert
        // status must remain true
        Assert.True(Program.everythingIsFine, "Status should remain true when the system is clean.");
    }
}

[Collection("Console Tests")]
public class LoggingMethodTests : IDisposable
{
    public readonly StringWriter _consoleOutput;
    public readonly TextWriter _originalOutput;

    public LoggingMethodTests()
    {
        // Arrange: Redirect Console.Out to capture the text written by WriteLogLine
        _consoleOutput = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    [Fact]
    public void WriteLogLine_FormatText_Test()
    {
        _consoleOutput.GetStringBuilder().Clear();
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, 
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        method.Invoke(null, new object[]{ConsoleColor.Green, "+", "Test Description"});

        _consoleOutput.Flush();
        
        string output = _consoleOutput.ToString().Trim();
        Assert.Equal("[+] Test Description", output);
    }

    [Fact]
    public void WriteLogLine_PreserveLeadingSpaces()
    {
        _consoleOutput.GetStringBuilder().Clear();
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null,
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        method.Invoke(null, new object[]{ConsoleColor.Yellow, "!", "     Update available"});

        string output = _consoleOutput.ToString().TrimEnd();
        Assert.StartsWith("     [!]", output);
        Assert.Contains("Update available", output);
    }

    [Fact]
    public void WriteLogLine_EmptyOverload_Test()
    {
        _consoleOutput.GetStringBuilder().Clear();
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        Assert.Equal('\u200b' + Environment.NewLine, _consoleOutput.ToString());
    }
    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }
}
