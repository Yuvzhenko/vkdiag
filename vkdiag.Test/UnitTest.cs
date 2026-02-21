using Xunit;
using VkDiag;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Xunit.Sdk;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("vkdiag.Test.PriorityOrderer", "vkdiag.Test")]

namespace vkdiag.Test;

//test order for correct working console output tests
public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) 
        where TTestCase : ITestCase
    {
        return testCases.OrderBy(testCase =>
        {
            var orderTrait = testCase.Traits.FirstOrDefault(t => t.Key == "Order");
            if (orderTrait.Value != null && int.TryParse(orderTrait.Value.FirstOrDefault(), out var priority))
            {
                return priority;
            }
            return 0;
        });
    }
}


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
    public void PackageIsFound()
    {
        // Arrange: Simulate that one package was found
        var packagesFound = new List<string> { "Fake_Incompatible_Package" };
        var serviceWithPackage = new FakePackageService(packagesFound);
        Program.everythingIsFine = true;

        //using reflection to get private mathod
        MethodInfo method = typeof(Program).GetMethod("CheckAppxPackages",
        BindingFlags.NonPublic | BindingFlags.Static, null,
        new[] {typeof(IPackageService)}, null);

        method.Invoke(null, new object[] {serviceWithPackage});

        // Assert
        Assert.False(Program.everythingIsFine);
    }

    [Fact]
    public void NoPackagesFound()
    {
        // Arrange: Simulate that NO packages were found (empty list)
        var noPackages = new List<string>(); 
        var emptyService = new FakePackageService(noPackages);
        Program.everythingIsFine = true;

        //using reflection to get private mathod
        MethodInfo method = typeof(Program).GetMethod("CheckAppxPackages",
        BindingFlags.NonPublic | BindingFlags.Static, null,
        new[] {typeof(IPackageService)}, null);

        method.Invoke(null, new object[] {emptyService});

        // Assert
        // status must remain true
        Assert.True(Program.everythingIsFine);
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

    [Fact, Trait("Order", "1")]
    public void FormatText_Test()
    {
        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, 
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        method.Invoke(null, new object[]{ConsoleColor.Green, "+", "Test Description"});

        //Veritify matches
        string output = _consoleOutput.ToString().Trim();
        Assert.Equal("[+] Test Description", output);
    }

    [Fact, Trait("Order", "2")]
    public void PreserveLeadingSpaces_Test()
    {
        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null,
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        //Using description with leading spaces
        method.Invoke(null, new object[]{ConsoleColor.Yellow, "!", "     Update available"});

        string output = _consoleOutput.ToString().TrimEnd();
        Assert.Contains("     [!] Update available", output);
    }

    [Fact, Trait("Order", "3")]
    public void EmptyOverload_Test()
    {
        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        Assert.Contains('\u200b' + Environment.NewLine, _consoleOutput.ToString());
    }
    public void Dispose()
    {
        //Restore the original console output
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }
}

public class GpuDriverInfoTests
{
    [Fact]
    public void FindVulkan_IntegrationTest()
    {
        //setting the global status to true
        Program.everythingIsFine = true;

        //calling the private CheckGpuDrivers() method and saving output
        MethodInfo method = typeof(Program).GetMethod("CheckGpuDrivers",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        var result = ((bool hasInactive, bool hasVulkan))method.Invoke(null, null);

        //checking if the program found Vulkan
        Assert.True(result.hasVulkan);

        //checking if the global status didn't change
        Assert.True(Program.everythingIsFine);
    }

    [Fact]
    public void Exception_Throwing_IntegrationTest()
    {
        //creating an exception and checking if the programs not crash
        MethodInfo method = typeof(Program).GetMethod("CheckGpuDrivers", 
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        //saving the expection
        var exception = Record.Exception(() => method.Invoke(null, null));
        
        Assert.Null(exception);
    }

    [Fact, Trait("Order", "4")]
    public void HardwareLog_IntegrationTest()
    {
        //relocating a console output
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("CheckGpuDrivers",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        //checking if the logs contains NVIDIA
        string output = sw.ToString();
        Assert.Contains("NVIDIA", output);

        //returning output to normal mode
        Console.SetOut(_originalOutput);
    }

    [Fact, Trait("Order", "5")]
    public void ShowUpdateWarning_IntegrationTest()
    {
        //relocating a console output
        using var sw = new System.IO.StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("CheckGpuDrivers",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        //expecting the warning
        string output = sw.ToString();
        Assert.Contains("Please consider updating your video driver", output);

        //returning output to normal mode
        Console.SetOut(originalOut);
    }

}

public class VulkanMetainfoTests
{
    [Fact, Trait("Order", "6")]
    public void CheckVulkanMeta_CorrectOutput_IntegrationTest()
    {
        //relocating a console output
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("CheckVulkanMeta",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        //saving the console output
        string output = sw.ToString();

        //matching the output with expections
        Assert.Contains("Vulkan registration information:", output);
        Assert.Contains("layers registration", output);
    }
    
    [Fact]
    public void CheckVulkanMeta_ExceptionThrowing_Test()
    {
        //Using redirection to get the private method
        MethodInfo method = typeof(Program).GetMethod("CheckVulkanMeta",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        //saving the expection
        var exception = Record.Exception(() => method.Invoke(null, null));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(@"{
        ""layer"": {
            ""name"": ""VK_LAYER_LUNARG_test"",
            ""api_version"": ""1.3.204"",
            ""description"": ""LunarG Test Layer vulkan layer"",
            ""library_path"": "".\\test.dll""
        }
    }", "LunarG Test", "1.3.204" )]
    [InlineData(@"{
        ""layer"": {
            ""name"": ""VK_LAYER_test"",
            ""api_version"": ""9.45.222"",
            ""description"": ""A Layer vulkan layer"",
            ""library_path"": "".\\test.dll""
        }
    }", "A", "9.45.222" )]
    [InlineData(@"{
        ""layer"": {
            ""name"": ""LAYER_LUNARG_test"",
            ""api_version"": ""0.0.114"",
            ""description"": ""The Test Layer vulkan layer"",
            ""library_path"": "".\\test.dll""
        }
    }", "The Test", "0.0.114" )]
    public void GetLayerInfo_ParseJSON_Test(string json_content, string expected_title, string version)
    {
        //converting versiong into the right type
        Version expected_apiVer = new Version(version);

        //creating a temp file
        string temp_json_path = Path.Combine(Path.GetTempPath(), "test_layer.json");
        File.WriteAllText(temp_json_path, json_content);
        try
        {
            //Using redirection to get the private method
            MethodInfo method = typeof(Program).GetMethod("GetLayerInfo",
            BindingFlags.NonPublic | BindingFlags.Static, null, new[] {typeof(string)}, null);

            //casting to untyped IEnumerable
            var rawResult = method.Invoke(null, new object[] {temp_json_path});
            var result = (System.Collections.IEnumerable)rawResult;

            //using dynamic to get Item1, Item2 and Item3
            dynamic first_entry = result.Cast<object>().First();

            //matching
            string title = first_entry.Item1;
            Assert.Contains(expected_title, title);
            Assert.DoesNotContain("vulkan layer", title);

            Version apiVer = first_entry.Item3;
            Assert.Equal(expected_apiVer.Major, apiVer.Major);
            Assert.Equal(expected_apiVer.Minor, apiVer.Minor);
        }
        finally
        {
            //deleting the temp file
            if (File.Exists(temp_json_path)) File.Delete(temp_json_path);
        }
    }
}
