namespace TextEditorProject.Core.Commands
{
    public sealed class CommandParameterDescriptor
    {
        public Type Type { get; }
        public bool IsOptional { get; }
        public string? Name { get; }
        public string? Description { get; }

        public CommandParameterDescriptor(Type type, bool isOptional = false, string? name = null, string? description = null)
        {
            Type = type;
            IsOptional = isOptional;
            Name = name;
            Description = description;
        }
    }
}