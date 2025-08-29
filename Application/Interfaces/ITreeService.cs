using TreeManagementApi.Application.DTOs;

namespace TreeManagementApi.Application.Interfaces;

/// <summary>
/// Service interface for tree management operations.
/// Encapsulates business logic for tree operations.
/// </summary>
public interface ITreeService
{
    /// <summary>
    /// Gets all available trees (returns list of trees with summary information)
    /// </summary>
    Task<IEnumerable<TreeSummaryDto>> GetAllTreesAsync();

    /// <summary>
    /// Gets a complete tree structure by tree ID
    /// </summary>
    /// <param name="treeId">ID of the tree to retrieve</param>
    /// <returns>Complete tree structure with all nodes</returns>
    Task<TreeDto> GetTreeAsync(int treeId);

    /// <summary>
    /// Creates a new tree with a root node
    /// </summary>
    /// <param name="request">Request containing the root node name</param>
    /// <returns>The created tree information</returns>
    Task<TreeDto> CreateTreeAsync(CreateTreeRequest request);

    /// <summary>
    /// Creates a new node in an existing tree
    /// </summary>
    /// <param name="treeId">ID of the tree to add the node to</param>
    /// <param name="request">Request containing parent ID and node name</param>
    /// <returns>The created node information</returns>
    Task<TreeNodeDto> CreateNodeAsync(int treeId, CreateNodeRequest request);

    /// <summary>
    /// Deletes a node from a tree. The node must not have any children.
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to delete</param>
    Task DeleteNodeAsync(int treeId, long nodeId);

    /// <summary>
    /// Renames a node in a tree
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to rename</param>
    /// <param name="request">Request containing the new name</param>
    /// <returns>The updated node information</returns>
    Task<TreeNodeDto> RenameNodeAsync(int treeId, long nodeId, RenameNodeRequest request);

    /// <summary>
    /// Gets a specific node by ID
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to retrieve</param>
    /// <returns>Node information</returns>
    Task<TreeNodeDto> GetNodeAsync(int treeId, long nodeId);

    /// <summary>
    /// Gets all children of a specific node
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the parent node</param>
    /// <returns>List of child nodes</returns>
    Task<IEnumerable<TreeNodeDto>> GetNodeChildrenAsync(int treeId, long nodeId);

    /// <summary>
    /// Gets the complete subtree starting from a specific node
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the root node of the subtree</param>
    /// <returns>Complete subtree structure</returns>
    Task<TreeDto> GetSubtreeAsync(int treeId, long nodeId);
}