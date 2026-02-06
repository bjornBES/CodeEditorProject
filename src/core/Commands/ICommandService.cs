namespace TextEditorProject.Core.Commands
{
    public interface ICommandService
    {
        Task ExecuteAsync(string commandId, params object[]? parameters);
        void Execute(string commandId, params object[]? parameters);
    }
}