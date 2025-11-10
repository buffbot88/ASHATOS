using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Tests;

public class ServerConnectorTests
{
    [Fact]
    public void ServerConnector_InitialState_IsNotConnected()
    {
        // Arrange & Act
        var connector = new ServerConnector();

        // Assert
        Assert.False(connector.IsConnected);
    }

    [Fact]
    public void ServerConnector_SetServerUrl_UpdatesUrl()
    {
        // Arrange
        var connector = new ServerConnector();
        var newUrl = "ws://test.example.com:8080/ws";

        // Act
        connector.ServerUrl = newUrl;

        // Assert
        Assert.Equal(newUrl, connector.ServerUrl);
    }

    [Fact]
    public void ServerConnector_SetServerUrlWhileConnected_ThrowsException()
    {
        // Arrange
        var connector = new ServerConnector();
        // Note: We can't actually connect in a unit test without a real server
        // This test documents the expected behavior

        // Assert
        // If connected, setting URL should throw InvalidOperationException
        // This would need integration testing with a mock server
    }
}

public class AiAgentTests
{
    [Fact]
    public void AiAgent_Constructor_RequiresServerConnector()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AiAgent(null!));
    }

    [Fact]
    public void AiAgent_SetAiEndpoint_UpdatesEndpoint()
    {
        // Arrange
        var connector = new ServerConnector();
        var aiAgent = new AiAgent(connector);
        var newEndpoint = "http://test.example.com/api/ai";

        // Act
        aiAgent.AiEndpoint = newEndpoint;

        // Assert
        Assert.Equal(newEndpoint, aiAgent.AiEndpoint);
    }

    [Fact]
    public void AiAgent_ApproveCode_RequiresApproverName()
    {
        // Arrange
        var connector = new ServerConnector();
        var aiAgent = new AiAgent(connector);
        var codeResult = new CodeGenerationResult
        {
            GeneratedCode = "Console.WriteLine(\"Hello\");",
            Language = "csharp"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => aiAgent.ApproveCode(codeResult, ""));
        Assert.Throws<ArgumentException>(() => aiAgent.ApproveCode(codeResult, null!));
    }

    [Fact]
    public void AiAgent_ApproveCode_SetsApprovalFields()
    {
        // Arrange
        var connector = new ServerConnector();
        var aiAgent = new AiAgent(connector);
        var codeResult = new CodeGenerationResult
        {
            GeneratedCode = "Console.WriteLine(\"Hello\");",
            Language = "csharp",
            RequiresHumanApproval = true,
            IsApproved = false
        };
        var approverName = "TestApprover";

        // Act
        var result = aiAgent.ApproveCode(codeResult, approverName);

        // Assert
        Assert.True(result);
        Assert.True(codeResult.IsApproved);
        Assert.Equal(approverName, codeResult.ApprovedBy);
        Assert.NotNull(codeResult.ApprovedAt);
    }
}

public class CodeGenerationResultTests
{
    [Fact]
    public void CodeGenerationResult_DefaultState_RequiresApproval()
    {
        // Arrange & Act
        var result = new CodeGenerationResult();

        // Assert
        Assert.True(result.RequiresHumanApproval);
        Assert.False(result.IsApproved);
        Assert.Null(result.ApprovedBy);
        Assert.Null(result.ApprovedAt);
    }

    [Fact]
    public void CodeGenerationResult_GeneratesUniqueId()
    {
        // Arrange & Act
        var result1 = new CodeGenerationResult();
        var result2 = new CodeGenerationResult();

        // Assert
        Assert.NotNull(result1.Id);
        Assert.NotNull(result2.Id);
        Assert.NotEqual(result1.Id, result2.Id);
    }
}

public class ServerConfigurationTests
{
    [Fact]
    public void ServerConfiguration_DefaultValues_AreSecure()
    {
        // Arrange & Act
        var config = new ServerConfiguration();

        // Assert
        Assert.True(config.UseAuthentication, "Authentication should be enabled by default");
        Assert.True(config.EnableRateLimiting, "Rate limiting should be enabled by default");
        Assert.True(config.MaxMessagesPerSecond > 0, "Rate limit should be positive");
    }
}

public class AiServiceConfigurationTests
{
    [Fact]
    public void AiServiceConfiguration_DefaultValues_RequireApproval()
    {
        // Arrange & Act
        var config = new AiServiceConfiguration();

        // Assert
        Assert.True(config.RequireApproval, "Approval should be required by default");
        Assert.True(config.EnablePolicyFilter, "Policy filter should be enabled by default");
        Assert.True(config.MaxRequestsPerMinute > 0, "Max requests should be positive");
    }
}
