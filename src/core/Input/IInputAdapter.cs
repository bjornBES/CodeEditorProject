using TextEditorProject.Core.Commands;

namespace TextEditorProject.Core.Input
{
    public interface IInputAdapter
    {
        void KeyDown(InputEvent e);
        void KeyUp(InputEvent e);
        void TextInput(string str);

        // mouse later
    }

    public class EditorInputAdapter : IInputAdapter
    {
        private readonly IEditorEngine engine;

        public EditorInputAdapter(IEditorEngine engine)
        {
            this.engine = engine;
        }

        public void KeyDown(InputEvent key)
        {
            if (key.Modifiers == Modifiers.None)
            {
                switch (key.Key)
                {
                    case Key.Left:
                        engine.Execute("cursor.left");
                        break;
                    case Key.Right:
                        engine.Execute("cursor.right");
                        break;
                    case Key.Up:
                        engine.Execute("cursor.up");
                        break;
                    case Key.Down:
                        engine.Execute("cursor.down");
                        break;
                }
            }

            if (key.Key == Key.Delete)
                engine.Execute("editor.delete.forward");

            if (key.Key == Key.Backspace)
                engine.Execute("editor.delete.backward");

            if (key.Key == Key.Enter)
                engine.Execute("editor.insert.newLine");
        }

        public void KeyUp(InputEvent e)
        {
        }

        public void TextInput(string str)
        {
            engine.Execute("editor.insertText", str);
        }
    }
}