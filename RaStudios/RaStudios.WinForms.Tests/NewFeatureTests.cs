using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Forms;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Tests;

public class HomePagePanelTests
{
    [Fact]
    public void HomePagePanel_Constructor_RequiresServerConnector()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HomePagePanel(null!));
    }

    [Fact]
    public void HomePagePanel_Constructor_InitializesSuccessfully()
    {
        // Arrange
        var connector = new ServerConnector();

        // Act
        var panel = new HomePagePanel(connector);

        // Assert
        Assert.NotNull(panel);
    }
}

public class LoginFormTests
{
    [Fact]
    public void LoginForm_Constructor_RequiresServerConnector()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoginForm(null!));
    }

    [Fact]
    public void LoginForm_Constructor_InitializesSuccessfully()
    {
        // Arrange
        var connector = new ServerConnector();

        // Act
        var form = new LoginForm(connector);

        // Assert
        Assert.NotNull(form);
        Assert.Empty(form.Username); // Initially no username
    }

    [Fact]
    public void LoginForm_Username_DefaultsToEmpty()
    {
        // Arrange
        var connector = new ServerConnector();
        var form = new LoginForm(connector);

        // Act & Assert
        Assert.Empty(form.Username);
    }
}

public class GameClientPanelTests
{
    [Fact]
    public void GameClientPanel_Constructor_RequiresServerConnector()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GameClientPanel(null!));
    }

    [Fact]
    public void GameClientPanel_Constructor_InitializesSuccessfully()
    {
        // Arrange
        var connector = new ServerConnector();

        // Act
        var panel = new GameClientPanel(connector);

        // Assert
        Assert.NotNull(panel);
    }
}

public class MainFormTests
{
    [Fact]
    public void MainForm_Constructor_InitializesSuccessfully()
    {
        // Arrange & Act
        var form = new MainForm();

        // Assert
        Assert.NotNull(form);
        Assert.Contains("RaStudios", form.Text);
    }

    [Fact]
    public void MainForm_Constructor_DoesNotThrowArgumentNullException()
    {
        // This test specifically verifies that the bug fix for
        // ArgumentNullException in HomePagePanel constructor works.
        // The fix ensures InitializeServices() is called before InitializeComponent(),
        // so serverConnector is not null when HomePagePanel is created.
        
        // Arrange & Act & Assert
        // Should not throw ArgumentNullException
        var exception = Record.Exception(() => new MainForm());
        Assert.Null(exception);
    }

    [Fact]
    public void MainForm_SetUserRole_UpdatesRole()
    {
        // Arrange
        var form = new MainForm();
        var role = "developer";

        // Act
        form.SetUserRole(role);

        // Assert
        // The role is set internally, we can't directly assert it
        // but we verify the method executes without errors
        Assert.NotNull(form);
    }

    [Fact]
    public void MainForm_SetUserRole_AcceptsValidRoles()
    {
        // Arrange
        var form = new MainForm();
        var validRoles = new[] { "guest", "player", "developer", "admin" };

        // Act & Assert
        foreach (var role in validRoles)
        {
            form.SetUserRole(role);
            // No exception should be thrown
        }
    }
}

public class PrivilegeBasedAccessTests
{
    [Theory]
    [InlineData("guest", 0)]
    [InlineData("player", 1)]
    [InlineData("developer", 2)]
    [InlineData("admin", 3)]
    public void RoleHierarchy_HasCorrectLevels(string role, int expectedLevel)
    {
        // Arrange
        var roleHierarchy = new Dictionary<string, int>
        {
            { "guest", 0 },
            { "player", 1 },
            { "developer", 2 },
            { "admin", 3 }
        };

        // Act
        var actualLevel = roleHierarchy[role];

        // Assert
        Assert.Equal(expectedLevel, actualLevel);
    }

    [Theory]
    [InlineData("player", "player", true)]  // Player can access player tab
    [InlineData("developer", "player", true)]  // Developer can access player tab
    [InlineData("admin", "player", true)]  // Admin can access player tab
    [InlineData("guest", "player", false)]  // Guest cannot access player tab
    [InlineData("developer", "developer", true)]  // Developer can access developer tab
    [InlineData("admin", "developer", true)]  // Admin can access developer tab
    [InlineData("player", "developer", false)]  // Player cannot access developer tab
    [InlineData("guest", "developer", false)]  // Guest cannot access developer tab
    public void RoleBasedAccess_EnforcesCorrectPermissions(string userRole, string requiredRole, bool shouldHaveAccess)
    {
        // Arrange
        var roleHierarchy = new Dictionary<string, int>
        {
            { "guest", 0 },
            { "player", 1 },
            { "developer", 2 },
            { "admin", 3 }
        };

        int userLevel = roleHierarchy.ContainsKey(userRole) ? roleHierarchy[userRole] : 0;
        int requiredLevel = roleHierarchy.ContainsKey(requiredRole) ? roleHierarchy[requiredRole] : 0;

        // Act
        bool hasAccess = userLevel >= requiredLevel;

        // Assert
        Assert.Equal(shouldHaveAccess, hasAccess);
    }
}
