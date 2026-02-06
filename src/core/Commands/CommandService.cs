namespace TextEditorProject.Core.Commands
{
    public sealed class CommandService : ICommandService
    {
        private readonly ICommandRegistry _registry;
        private readonly CommandContext _context;

        public CommandService(ICommandRegistry registry, CommandContext context)
        {
            _registry = registry;
            _context = context;
        }

        public async Task ExecuteAsync(string commandId, params object[]? parameters)
        {
            if (!_registry.TryGet(commandId, out var descriptor))
                throw new KeyNotFoundException($"Command '{commandId}' not found.");

            if (descriptor.CanExecute != null && !descriptor.CanExecute(_context))
                throw new CommandExecutionException($"Command '{commandId}' cannot be executed in the current context.");


            var args = parameters ?? Array.Empty<object>();

            if (descriptor.Parameters != null)
            {
                int minCount = descriptor.Parameters.Count(p => !p.IsOptional);
                int maxCount = descriptor.Parameters.Count;

                if (args.Length < minCount || args.Length > maxCount)
                {
                    throw new CommandArgumentException(
                        $"Invalid parameters for command '{commandId}'. Expected {minCount}–{maxCount} arguments.");
                }

                for (int i = 0; i < args.Length; i++)
                {
                    var expected = descriptor.Parameters[i];

                    if (args[i] is null)
                    {
                        if (!expected.IsOptional)
                            throw new CommandArgumentException($"Invalid parameters for command '{commandId}'. Parameters is null.");
                    }
                    else if (!expected.Type.IsInstanceOfType(args[i]))
                    {
                        throw new CommandArgumentException($"Invalid parameters for command '{commandId}'. Expected parameter '{expected.Name ?? i.ToString()}' to be of type {expected.Type.Name}.");
                    }
                }
            }

            await descriptor.Handler(_context, args);
        }

        public void Execute(string commandId, params object[]? parameters)
        {
            if (!_registry.TryGet(commandId, out var descriptor))
                throw new KeyNotFoundException($"Command '{commandId}' not found.");

            if (descriptor.CanExecute != null && !descriptor.CanExecute(_context))
                throw new CommandExecutionException($"Command '{commandId}' cannot be executed in the current context.");


            var args = parameters ?? Array.Empty<object>();

            if (descriptor.Parameters != null)
            {
                int minCount = descriptor.Parameters.Count(p => !p.IsOptional);
                int maxCount = descriptor.Parameters.Count;

                if (args.Length < minCount || args.Length > maxCount)
                {
                    throw new CommandArgumentException(
                        $"Invalid parameters for command '{commandId}'. Expected {minCount}–{maxCount} arguments.");
                }

                for (int i = 0; i < args.Length; i++)
                {
                    var expected = descriptor.Parameters[i];

                    if (args[i] is null)
                    {
                        if (!expected.IsOptional)
                            throw new CommandArgumentException($"Invalid parameters for command '{commandId}'. Parameters is null.");
                    }
                    else if (!expected.Type.IsInstanceOfType(args[i]))
                    {
                        throw new CommandArgumentException($"Invalid parameters for command '{commandId}'. Expected parameter '{expected.Name ?? i.ToString()}' to be of type {expected.Type.Name}.");
                    }
                }
            }

            descriptor.Handler(_context, args);
        }
    }
}