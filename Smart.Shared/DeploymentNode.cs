namespace Smart.Shared;

public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;
    public List<DeploymentNode> Children { get; set; } = new();

    public DeploymentNode() { }
    public DeploymentNode(string name) => Name = name;

    // RECURSION: Walks the tree to find and validate a target node
    public static bool ValidateDeploymentPath(DeploymentNode node, string targetName)
    {
        if (node.Name == targetName) return true;

        foreach (var child in node.Children)
        {
            if (ValidateDeploymentPath(child, targetName)) return true;
        }
        return false;
    }
}