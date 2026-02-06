namespace TextEditorProject.Core.Commands
{
    public sealed class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<string, CommandDescriptor> _commands = new();


        public void Register(CommandDescriptor command)
        {
            if (_commands.ContainsKey(command.Id))
                throw new InvalidOperationException($"Command '{command.Id}' already registered.");

            _commands[command.Id] = command;
        }

        public bool TryGet(string id, out CommandDescriptor command)
            => _commands.TryGetValue(id, out command!);

        public IEnumerable<CommandDescriptor> GetAll() => _commands.Values;
    }
}