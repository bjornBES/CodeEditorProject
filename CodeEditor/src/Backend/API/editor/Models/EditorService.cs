
public class EditorService
{
    private readonly Dictionary<EditorInput, DocumentLspSession> lspSessions = new();
    private readonly List<EditorGroup> groups = new();
    public IReadOnlyList<EditorGroup> Groups => groups;
    public EditorGroup ActiveGroup { get; private set; }

    public EditorService()
    {
        EditorGroup rootGroup = new EditorGroup();
        groups.Add(rootGroup);
        ActiveGroup = rootGroup;

    }

    public void OpenEditor(EditorBuffer buffer, bool pinned = false)
    {
        ActiveGroup.OpenEditor(buffer, pinned);

        if (buffer.Input is FileEditorInput fileInput)
        {
            ILanguageClient? client = LspManager.GetClientForFile(fileInput.FilePath);
            if (client != null)
            {
                var session = new DocumentLspSession(fileInput, client);
                lspSessions[fileInput] = session;
                _ = session.OpenAsync(); // fire & forget or handle properly
            }
        }
    }

    public void CloseEditor(EditorInput input)
    {
        ActiveGroup.CloseEditor(input);
    }
}
