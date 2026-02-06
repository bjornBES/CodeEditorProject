namespace TextEditorProject.Core.Commands
{
    [Serializable]
    internal class CommandArgumentException : Exception
    {
        private Range range;

        public CommandArgumentException()
        {
        }

        public CommandArgumentException(Range range)
        {
            this.range = range;
        }

        public CommandArgumentException(string? message) : base(message)
        {
        }

        public CommandArgumentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}