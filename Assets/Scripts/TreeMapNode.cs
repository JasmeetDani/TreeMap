using System.Collections.Generic;

public class TreeMapNode
{
    public int ID { get; set; }


    public TreeMapNodeData Data { get; set; }


    public TreeMapNode Parent { get; set; }


    private List<TreeMapNode> children = new List<TreeMapNode>();

    public List<TreeMapNode> Children
    {
        get { return children; }

        private set { }
    }
    
    public void AddChild(TreeMapNode node)
    {
        children.Add(node);
    }
}