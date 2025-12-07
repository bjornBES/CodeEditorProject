using Xunit;
using CodeEditor;

namespace CodeEditor.Tests;

public class BasicTests
{
    [Fact]
    public void CanCreateWorkspace()
    {
        Workspace workspace = new Workspace("testWorkspace", "./testScr");

        WorkspaceManager.SaveWorkspace(workspace);

        workspace = WorkspaceManager.OpenWorkspace("testWorkspace");
        Assert.True(workspace != null);
    }
    [Fact]
    public void CanCreateNewDoc()
    {
        Workspace workspace = WorkspaceManager.OpenWorkspace("testWorkspace");
        
        if (workspace == null)
        {
            Assert.Fail("Could not open workspace");
        }

        bool result = DocumentManager.CreateNewDocument("test.txt");

        Assert.True(result);
    }
}
