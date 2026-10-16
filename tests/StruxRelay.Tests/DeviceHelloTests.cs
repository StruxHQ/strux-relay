using System.Text;
using StruxRelay.Models;

namespace StruxRelay.Tests;

/// <summary>
/// A device on the channels wire sends no hello; the relay builds one from its
/// <c>system info</c> and <c>system describe</c> replies. What these check is that
/// the result reads exactly like a hello would, so nothing downstream has to care
/// which wire a device speaks.
/// </summary>
public class DeviceHelloTests
{
    private static DeviceHello Parse(string json) =>
        DeviceHello.Parse(Encoding.UTF8.GetBytes(json))!;

    private const string Info =
        """{"name":"KC-OT-Testbench","project":"KC-OT-Testbench","firmware":"0.0.0-dev","ip":"192.168.50.145","heapFree":4247104}""";

    private const string Describe =
        """{"ok":true,"name":"KC-OT-Testbench","firmware":"0.0.0-dev","commit":"9c0168f-dirty","description":"A testbench.","instructions":"Pages of prose."}""";

    [Fact]
    public void System_info_fills_the_fields_a_hello_would()
    {
        var hello = DeviceHello.FromSystemCommands(Parse(Info), describe: null);

        Assert.Equal("KC-OT-Testbench", hello.Name);
        Assert.Equal("KC-OT-Testbench", hello.Project);
        Assert.Equal("0.0.0-dev", hello.Firmware);
        Assert.Equal("192.168.50.145", hello.Ip);
        Assert.False(hello.Values.ContainsKey("firmware"));
    }

    [Fact]
    public void System_describe_adds_the_commit_and_the_description_and_nothing_else()
    {
        var hello = DeviceHello.FromSystemCommands(Parse(Info), Parse(Describe));

        Assert.Equal("9c0168f-dirty", hello.Commit);
        Assert.Equal("A testbench.", hello.Description);
        Assert.False(hello.Values.ContainsKey("instructions"));
        Assert.False(hello.Values.ContainsKey("ok"));
    }
}
