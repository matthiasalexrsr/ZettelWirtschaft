using System.Text.RegularExpressions;

namespace DocuDesk.Tests.UI;

public sealed class ThemeContractTests
{
    [Fact]
    public void AppXaml_MergesExpectedGlobalDictionaries()
    {
        var appXaml = ReadRepoFile("src/DocuDesk.Desktop/App.xaml");

        Assert.Contains("Resources/DocuDesk.DesignTokens.xaml", appXaml);
        Assert.Contains("Resources/DocuDesk.ModernTheme.xaml", appXaml);
    }

    [Fact]
    public void MainWindowXaml_PreservesCriticalBindingsAndViewerName()
    {
        var mainWindow = ReadRepoFile("src/DocuDesk.Desktop/MainWindow.xaml");

        Assert.Contains("x:Name=\"DocumentViewer\"", mainWindow);
        Assert.Contains("Command=\"{Binding SearchCommand}\"", mainWindow);
        Assert.Contains("Command=\"{Binding ImportCommand}\"", mainWindow);
        Assert.Contains("Command=\"{Binding ProcessJobsCommand}\"", mainWindow);
        Assert.Contains("Command=\"{Binding BackupCommand}\"", mainWindow);
        Assert.Contains("Command=\"{Binding DraftMailCommand}\"", mainWindow);
        Assert.Contains("ItemsSource=\"{Binding Documents}\"", mainWindow);
        Assert.Contains("SelectedItem=\"{Binding SelectedDocument}\"", mainWindow);
        Assert.Contains("ItemsSource=\"{Binding Jobs}\"", mainWindow);
        Assert.Contains("SourcePath=\"{Binding SelectedDocumentPath}\"", mainWindow);
        Assert.Contains("OverlayJson=\"{Binding SelectedDocumentOverlayJson}\"", mainWindow);
        Assert.Contains("HighlightQuery=\"{Binding SearchText}\"", mainWindow);
    }

    [Fact]
    public void MainWindowXaml_UsesThemeStylesForCoreShellAreas()
    {
        var mainWindow = ReadRepoFile("src/DocuDesk.Desktop/MainWindow.xaml");
        var styleKeys = Regex.Matches(mainWindow, "\\{StaticResource\\s+([A-Za-z0-9_.-]+)\\}")
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("CardBorderStyle", styleKeys);
        Assert.Contains("PrimaryButtonStyle", styleKeys);
        Assert.Contains("SidebarButtonStyle", styleKeys);
        Assert.Contains("SectionHeaderTextStyle", styleKeys);
        Assert.Contains("DocumentListItemStyle", styleKeys);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = GetRepoRoot();
        return File.ReadAllText(Path.Combine(repoRoot, relativePath));
    }

    private static string GetRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test runtime directory.");
    }
}
