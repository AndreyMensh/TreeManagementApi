using TreeManagementApi.Domain.Entities;

namespace TreeManagementApi.Application.Interfaces;

/// <summary>
/// Repository interface for TreeNode operations
/// </summary>
public interface ITreeNodeRepository
{
    /// <summary>
    /// Gets all nodes in a specific tree
    /// </summary>
    Task<IEnumerable<TreeNode>> GetTreeAsync(int treeId);

    /// <summary>
    /// Gets a specific node by ID and TreeId
    /// </summary>
    Task<TreeNode?> GetNodeAsync(int treeId, long nodeId);

    /// <summary>
    /// Gets all root nodes (nodes without parents) grouped by TreeId
    /// </summary>
    Task<IEnumerable<TreeNode>> GetRootNodesAsync();

    /// <summary>
    /// Gets all root nodes for a specific tree
    /// </summary>
    Task<IEnumerable<TreeNode>> GetRootNodesAsync(int treeId);

    /// <summary>
    /// Gets all children of a specific node
    /// </summary>
    Task<IEnumerable<TreeNode>> GetChildrenAsync(int treeId, long parentId);

    /// <summary>
    /// Gets the entire subtree starting from a specific node (using materialized path)
    /// </summary>
    Task<IEnumerable<TreeNode>> GetSubtreeAsync(int treeId, long nodeId);

    /// <summary>
    /// Checks if a node has any children
    /// </summary>
    Task<bool> HasChildrenAsync(int treeId, long nodeId);

    /// <summary>
    /// Checks if a tree exists
    /// </summary>
    Task<bool> TreeExistsAsync(int treeId);

    /// <summary>
    /// Gets all unique tree IDs
    /// </summary>
    Task<IEnumerable<int>> GetAllTreeIdsAsync();

    /// <summary>
    /// Gets the next available TreeId
    /// </summary>
    Task<int> GetNextTreeIdAsync();

    /// <summary>
    /// Adds a new node to the repository
    /// </summary>
    Task<TreeNode> AddNodeAsync(TreeNode node);

    /// <summary>
    /// Updates an existing node
    /// </summary>
    Task<TreeNode> UpdateNodeAsync(TreeNode node);

    /// <summary>
    /// Deletes a node from the repository
    /// </summary>
    Task DeleteNodeAsync(TreeNode node);

    /// <summary>
    /// Updates the materialized path for a node and all its descendants
    /// </summary>
    Task UpdatePathsAsync(TreeNode node);

    /// <summary>
    /// Validates that a parent node exists and belongs to the same tree
    /// </summary>
    Task<bool> ValidateParentAsync(int treeId, long parentId);

    /// <summary>
    /// Checks if setting a parent would create a circular reference
    /// </summary>
    Task<bool> WouldCreateCircularReferenceAsync(int treeId, long nodeId, long parentId);
}

/// <summary>
/// Repository interface for ExceptionJournal operations
/// </summary>
public interface IExceptionJournalRepository
{
    /// <summary>
    /// Adds a new exception log entry
    /// </summary>
    Task<ExceptionJournal> AddExceptionAsync(ExceptionJournal exceptionJournal);

    /// <summary>
    /// Gets an exception log by ID
    /// </summary>
    Task<ExceptionJournal?> GetExceptionAsync(long id);

    /// <summary>
    /// Gets recent exception logs
    /// </summary>
    Task<IEnumerable<ExceptionJournal>> GetRecentExceptionsAsync(int count = 100);

    /// <summary>
    /// Gets exception logs by type
    /// </summary>
    Task<IEnumerable<ExceptionJournal>> GetExceptionsByTypeAsync(string exceptionType, int count = 50);
}

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ITreeNodeRepository TreeNodes { get; }
    IExceptionJournalRepository ExceptionJournals { get; }

    /// <summary>
    /// Saves all changes in a single transaction
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}