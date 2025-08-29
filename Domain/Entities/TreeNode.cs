using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeManagementApi.Domain.Entities;

/// <summary>
/// Represents a node in a hierarchical tree structure.
/// Each node belongs to a specific tree (TreeId) and can have a parent node (ParentId).
/// The Path field stores the materialized path for efficient querying of subtrees.
/// </summary>
public class TreeNode
{
    /// <summary>
    /// Unique identifier for the node within the tree
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Name of the node - required field with maximum length of 255 characters
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the tree this node belongs to.
    /// All nodes in the same tree must have the same TreeId.
    /// This ensures tree isolation and prevents cross-tree references.
    /// </summary>
    [Required]
    public int TreeId { get; set; }

    /// <summary>
    /// Foreign key to the parent node within the same tree.
    /// Null for root nodes.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Materialized path for efficient subtree queries.
    /// Format: "1.3.7." - dot-separated sequence of node IDs from root to current node.
    /// This improves performance when querying entire subtrees.
    /// </summary>
    [StringLength(4000)] // Sufficient for deep hierarchies
    public string? Path { get; set; }

    /// <summary>
    /// Timestamp when the node was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to parent node
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public virtual TreeNode? Parent { get; set; }

    /// <summary>
    /// Navigation property to child nodes
    /// </summary>
    public virtual ICollection<TreeNode> Children { get; set; } = new List<TreeNode>();

    /// <summary>
    /// Determines if this node is a root node (has no parent)
    /// </summary>
    [NotMapped]
    public bool IsRoot => ParentId == null;

    /// <summary>
    /// Gets the depth level of the node in the tree (root = 0)
    /// </summary>
    [NotMapped]
    public int Level => Path?.Split('.', StringSplitOptions.RemoveEmptyEntries).Length - 1 ?? 0;

    /// <summary>
    /// Updates the materialized path based on parent's path
    /// </summary>
    public void UpdatePath()
    {
        if (Parent == null)
        {
            Path = $"{Id}.";
        }
        else
        {
            Path = $"{Parent.Path}{Id}.";
        }
    }

    /// <summary>
    /// Checks if this node is an ancestor of the specified node
    /// </summary>
    /// <param name="node">Node to check</param>
    /// <returns>True if this node is an ancestor of the specified node</returns>
    public bool IsAncestorOf(TreeNode node)
    {
        return node.Path?.StartsWith(Path ?? string.Empty) == true && node.Id != Id;
    }

    /// <summary>
    /// Checks if this node is a descendant of the specified node
    /// </summary>
    /// <param name="node">Node to check</param>
    /// <returns>True if this node is a descendant of the specified node</returns>
    public bool IsDescendantOf(TreeNode node)
    {
        return Path?.StartsWith(node.Path ?? string.Empty) == true && Id != node.Id;
    }
}