namespace TextEditorProject.Core.Commands
{
    public static class CommandHelper
    {
        public static CommandDescriptor Create<T1, T2, T3>(string id, Func<CommandContext, T1, T2, T3, Task> handler, string? title = null, string? description = null)
        {
            return new CommandDescriptor(
                id,
                async (ctx, args) =>
                {
                    await handler(ctx, (T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
                },
                title,
                description,
                new CommandParameterDescriptor(typeof(T1)),
                new CommandParameterDescriptor(typeof(T2)),
                new CommandParameterDescriptor(typeof(T3)));
        }
        public static CommandDescriptor Create<T1, T2>(string id, Func<CommandContext, T1, T2, Task> handler, string? title = null, string? description = null)
        {
            return new CommandDescriptor(
                id,
                async (ctx, args) =>
                {
                    await handler(ctx, (T1)args[0]!, (T2)args[1]!);
                },
                title,
                description,
                new CommandParameterDescriptor(typeof(T1)),
                new CommandParameterDescriptor(typeof(T2)));
        }
        public static CommandDescriptor Create<T1>(string id, Func<CommandContext, T1, Task> handler, string? title = null, string? description = null)
        {
            return new CommandDescriptor(
                id,
                async (ctx, args) =>
                {
                    await handler(ctx, (T1)args[0]!);
                },
                title,
                description,
                new CommandParameterDescriptor(typeof(T1)));
        }
        public static CommandDescriptor Create(string id, Func<CommandContext, Task> handler, string? title = null, string? description = null)
        {
            return new CommandDescriptor(
                id,
                async (ctx, args) =>
                {
                    await handler(ctx);
                },
                title,
                description);
        }
    }
}