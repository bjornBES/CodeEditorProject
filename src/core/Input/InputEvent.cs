namespace TextEditorProject.Core.Input
{
    public readonly struct InputEvent
    {
        public Key Key { get; }
        public Modifiers Modifiers { get; }
        public bool IsRepeat { get; }
        public InputEvent(Key key, Modifiers modifiers, bool isRepeat)
        {
            Key = key;
            Modifiers = modifiers;
            IsRepeat = isRepeat;
        }
    }
}