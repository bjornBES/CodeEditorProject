using TextEditorProject.Core.Commands;
using TextEditorProject.Core.Input;
using TextEditorProject.Core.Snapshots;

namespace TextEditorProject.Core.Engine
{
    public sealed class EditorEngine : IEditorEngine
    {
        private EditorState _state;

        private readonly CommandRegistry _registry;
        private readonly CommandService _commands;

        private readonly IInputAdapter _inputAdapter;

        public IEngineServices EngineServices;

        public EditorEngine(IHostServices host)
        {
            _registry = new CommandRegistry();

            var context = new CommandContext(host, EngineServices, _state);

            _commands = new CommandService(_registry, context);
            context.Commands = _commands;

            _inputAdapter = new EditorInputAdapter(this);
            _state = new EditorState(this);
        }

        public void Execute(string command, params object[] args)
        {
            _commands.Execute(command, args);
        }

        public EditorSnapshot Snapshot()
        {
            return EditorSnapshot.FromState(_state);
        }

        public ICommandRegistry Registry => _registry;
        public ICommandService Commands => _commands;
        public IInputAdapter InputAdapter => _inputAdapter;
    }
}