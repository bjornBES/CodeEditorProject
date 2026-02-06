#nullable disable
namespace TextEditorProject.Rendering.UI.Abstractions
{
    public static class UiNodeExtensions
    {
        public static T Find<T>(this UiNode root, string id) where T : UiNode
        {
            if (root.Id == id && root is T match)
                return match;

            if (root is UiContainerNode container)
            {
                foreach (var child in container.Children)
                {
                    var result = child.Find<T>(id);
                    if (result != null)
                        return result;
                }
            }

            return null!;
        }
        public static T Also<T>(this T root, Action<T> action) where T : UiNode
        {
            action.Invoke(root);
            return root;
        }
        public static void Add<T>(this T root, UiNode[] nodes) where T : UiContainerNode
        {
            foreach(var node in nodes)
            {
                root.Add(node);
            }
        }
    }

}