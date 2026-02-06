using TextEditorProject.Core.Engine;
namespace TextEditorProject.Core.Commands
{
    public sealed class CommandContext
    {
        public IHostServices Host { get; }
        public IEngineServices Engine { get; }
        public EditorState EditorState { get; }
        public ICommandService Commands { get; internal set; }
#nullable disable
        public CommandContext(IHostServices host, IEngineServices engine, EditorState editorState)
        {
            Host = host;
            Engine = engine;
            EditorState = editorState;
        }
#nullable restore
    }

}