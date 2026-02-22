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
        //Redirect Console.Out to capture the text written by WriteLogLine
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);
        
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

        //returning output to normal mode
        Console.SetOut(_originalOutput);
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
public class LoggingMethodTests
{

    [Fact, Trait("Order", "1")]
    public void FormatText_Test()
    {
        //Redirect Console.Out to capture the text written by WriteLogLine
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using reflection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, 
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        method.Invoke(null, new object[]{ConsoleColor.Green, "+", "Test Description"});

        //Veritify matches
        string output = sw.ToString().Trim();
        Assert.Equal("[+] Test Description", output);

        //returning output to normal mode
        Console.SetOut(_originalOutput);
    }

    [Fact, Trait("Order", "2")]
    public void PreserveLeadingSpaces_Test()
    {
        //Redirect Console.Out to capture the text written by WriteLogLine
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using reflection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null,
         new[] {typeof(ConsoleColor), typeof(string), typeof(string)}, null);
        
        //Using description with leading spaces
        method.Invoke(null, new object[]{ConsoleColor.Yellow, "!", "     Update available"});

        string output = sw.ToString().TrimEnd();
        Assert.Contains("     [!] Update available", output);

        //returning output to normal mode
        Console.SetOut(_originalOutput);
    }

    [Fact, Trait("Order", "3")]
    public void EmptyOverload_Test()
    {
        //Redirect Console.Out to capture the text written by WriteLogLine
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);
        
        //Using reflection to get the private method
        MethodInfo method = typeof(Program).GetMethod("WriteLogLine",
         BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        Assert.Contains('\u200b' + Environment.NewLine, sw.ToString());

        //returning output to normal mode
        Console.SetOut(_originalOutput);
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
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        sw.GetStringBuilder().Clear();

        //creating an exception and checking if the programs not crash
        MethodInfo method = typeof(Program).GetMethod("CheckGpuDrivers", 
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        //saving the expection
        var exception = Record.Exception(() => method.Invoke(null, null));
        
        Assert.Null(exception);

        Console.SetOut(_originalOutput);
    }

    [Fact, Trait("Order", "4")]
    public void HardwareLog_IntegrationTest()
    {
        //relocating a console output
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using reflection to get the private method
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

        //Using reflection to get the private method
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

        //Using reflection to get the private method
        MethodInfo method = typeof(Program).GetMethod("CheckVulkanMeta",
        BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        method.Invoke(null, null);

        //saving the console output
        string output = sw.ToString();

        //matching the output with expections
        Assert.Contains("Vulkan registration information:", output);
        Assert.Contains("layers registration", output);

        Console.SetOut(_originalOutput);
    }
    
    [Fact]
    public void CheckVulkanMeta_ExceptionThrowing_Test()
    {
        //Using reflection to get the private method
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
            //Using reflection to get the private method
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

public class OsInfoTests
{
    [Fact, Trait("Order", "7")]
    public void CheckOs_IntegrationTest()
    {
        //relocating a console output
        using var sw = new System.IO.StringWriter();
        var _originalOutput = Console.Out;
        Console.SetOut(sw);

        //Using reflection to get the private method 
        MethodInfo method = typeof(Program).GetMethod("CheckOs",
        BindingFlags.NonPublic | BindingFlags.Static);

        //saving the output
        var osVersion = (Version)method.Invoke(null, null);
        string output = sw.ToString();

        //matching
        Assert.NotNull(osVersion);
        Assert.True(osVersion.Major >= 10);

        Assert.Contains("CPU:", output);
        Assert.Contains("OS:", output);

        Assert.Contains("System Vulkan loader version:", output);

        //returning output to normal mode
        Console.SetOut(_originalOutput);
    }

    [Theory]
    //old versions
    [InlineData("5.1.2600", OsSupportStatus.Deprecated, "XP")]
    [InlineData("6.1.7601", OsSupportStatus.Deprecated, "7")]
    [InlineData("6.3.9600", OsSupportStatus.Deprecated, "8.1")]

    //Windows 10
    [InlineData("10.0.10240", OsSupportStatus.Deprecated, "10 1507")]
    [InlineData("10.0.19045", OsSupportStatus.Deprecated, "10 22H2")]

    //Windows 11 
    [InlineData("10.0.22631", OsSupportStatus.Deprecated, "11 23H2")]
    [InlineData("10.0.26100", OsSupportStatus.Supported, "11 24H2")]
    [InlineData("10.0.26300", OsSupportStatus.Prerelease, "11 25H2 Dev Build 26300")]

    //unknown systems
    [InlineData("11.0.0", OsSupportStatus.Unknown, null)]
    public void GetWindowsInfo_Test(string str_version, OsSupportStatus expected_status, string expected_name)
    {
        //convert version into correct type
        Version version = new Version(str_version);

        //using reflection to get private method
        MethodInfo method = typeof(Program).GetMethod("GetWindowsInfo",
        BindingFlags.NonPublic | BindingFlags.Static, null, new[]{typeof(Version)}, null);

        //getting a result
        var result = ((OsSupportStatus status, string name))method.Invoke(null, new object[] {version});

        //matching
        Assert.Equal(result.status, expected_status);
        if (expected_name == null)
            Assert.Null(result.name);
        else
            Assert.Equal(result.name, expected_name);
    }

    [Fact, Trait("Order", "8")]
    public void HasPerformanceModeProfile_EnsureProfileExists_IntegrationTest()
    {
        //using reflection to get a private method
        MethodInfo method = typeof(Program).GetMethod("HasPerformanceModeProfile",
        BindingFlags.NonPublic | BindingFlags.Static);

        //getting a path that will be used as a key in the register
        var imagePath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
        if (imagePath == null) return;

        //doing two runs to be sure that the value is true
        bool first_run = (bool)method.Invoke(null, null);
        bool second_run = (bool)method.Invoke(null, null);

        //matching
        Assert.True(second_run);

        //cleanup
        if(imagePath != null)
        {
            using var userGpuPrefs = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\DirectX\UserGpuPreferences", true);
            userGpuPrefs?.DeleteValue(imagePath, false);
        }
    }

    [Fact, Trait("Order", "9")]
    public void HasPerformanceModeProfile_FixIncorrectValue_IntegrationTest()
    {
        //getting a path that will be used as a key in the register
        var imagePath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
        if (imagePath == null) return;

        var registryPath = @"Software\Microsoft\DirectX\UserGpuPreferences";
        
        //creating incorrect value in the register
        using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath, true))
        {
            key.SetValue(imagePath, "GpuPreference=1;");
        }

        //using reflection to get a private method
        var method = typeof(Program).GetMethod("HasPerformanceModeProfile", 
            BindingFlags.NonPublic | BindingFlags.Static);

        //first call must detect incorrect value and return false
        bool wasCorrectBeforeFix = (bool)method.Invoke(null, null);

        //match
        Assert.False(wasCorrectBeforeFix);

        //checking if the value was changed
        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath))
        {
            var actualValue = key?.GetValue(imagePath) as string;
            Assert.Equal("GpuPreference=2;", actualValue);
        }

        //cleanup
        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath, true))
        {
            key?.DeleteValue(imagePath, false);
        }
    }
}
