
public class EditorBuffer
{
    public EditorControlView View;
    public EditorInput Input;
    public EditorBuffer(EditorInput input, EditorControlView view)
    {
        View = view;
        Input = input;
    }
}