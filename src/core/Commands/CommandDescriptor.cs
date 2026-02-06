namespace TextEditorProject.Core.Commands
{
    public sealed class CommandDescriptor
    {
        public string Id { get; }
        public string? Title { get; }
        public string? Description { get; }

        public Func<CommandContext, bool>? CanExecute { get; }
        public IReadOnlyList<CommandParameterDescriptor>? Parameters { get; }
        public Func<CommandContext, IReadOnlyList<object?>, Task> Handler { get; }

        public CommandDescriptor(string id, Func<CommandContext, IReadOnlyList<object?>, Task> handler, string? title = null, string? description = null, params CommandParameterDescriptor[]? parameters)
        {
            Id = id;
            Handler = handler;
            Parameters = parameters;
            Title = title;
            Description = description;
        }
    }
}