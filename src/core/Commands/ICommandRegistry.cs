namespace TextEditorProject.Core.Commands
{
    public interface ICommandRegistry
    {
        void Register(CommandDescriptor command);
        bool TryGet(string id, out CommandDescriptor command);
        IEnumerable<CommandDescriptor> GetAll();
    }
}