#nullable disable
using shared;
using TextEditorProject.Core.Editor.Models;

namespace TextEditorProject.Rendering.UI.Abstractions
{
    public abstract class UiNode
    {
        public string Id { get; }

        protected UiNode(string id)
        {
            Id = id;
        }
    }

    public abstract class UiContainerNode : UiNode
    {
        protected UiContainerNode(string id) : base(id) { }

        public abstract IReadOnlyList<UiNode> Children { get; }

        public abstract void Add(UiNode node);
    }

    public sealed class UiColumnNode : UiContainerNode
    {
        private readonly List<UiNode> _children = new();

        public UiColumnNode(string id) : base(id) { }

        public override IReadOnlyList<UiNode> Children => _children;

        public override void Add(UiNode node) => _children.Add(node);
    }

    public sealed class UiRowNode : UiContainerNode
    {
        private readonly List<UiNode> _children = new();

        public UiRowNode(string id) : base(id) { }

        public override IReadOnlyList<UiNode> Children => _children;

        public override void Add(UiNode node) => _children.Add(node);
    }


    public sealed class UiButtonNode : UiNode
    {
        public string Text { get; set; }
        public bool IsEnabled { get; set; }
        public Rect Bounds { get; set; }

        public UiButtonNode(string id) : base(id) { Bounds = new Rect(); }
    }

    public sealed class UiTextFieldNode : UiNode
    {
        public string Text { get; set; }
        public bool IsReadOnly { get; set; }

        public Rect Bounds { get; set; }

        public UiTextFieldNode(string id) : base(id) { Bounds = new Rect(); }
    }
    public sealed class UiEditorNode : UiNode
    {
        public Rect Bounds { get; set; }

        // identity (references, not ownership)
        public EditorId EditorId { get; set; }
        public DocumentId DocumentId { get; set; }

        // view state (UI-only)
        public int ScrollLine { get; set; }
        public int ScrollColumn { get; set; }

        public bool HasFocus { get; set; }

        public UiEditorNode(string id) : base(id)
        {
            Bounds = new Rect();
        }
    }
}