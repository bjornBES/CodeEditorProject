using TextEditorProject.Core.Commands;

namespace TextEditorProject.Core.Contracts
{
    public interface ICommandRegistrar
    {
        void Register(CommandDescriptor descriptor);
    }
}