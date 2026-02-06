using TextEditorProject.Core.Commands;
using TextEditorProject.Core.Input;
using TextEditorProject.Core.Snapshots;

public interface IEditorEngine
{
    void Execute(string command, params object[] args);
    EditorSnapshot Snapshot();

    ICommandService Commands { get; }
    ICommandRegistry Registry { get; }

    IInputAdapter InputAdapter{ get; }
}
