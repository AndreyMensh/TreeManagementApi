namespace TreeManagementApi.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested tree node is not found.
/// </summary>
public class NodeNotFoundException : SecureException
{
    public NodeNotFoundException(long nodeId) 
        : base($"Node with ID {nodeId} was not found.")
    {
    }

    public NodeNotFoundException(long nodeId, int treeId) 
        : base($"Node with ID {nodeId} was not found in tree {treeId}.")
    {
    }
}

/// <summary>
/// Exception thrown when a requested tree is not found.
/// </summary>
public class TreeNotFoundException : SecureException
{
    public TreeNotFoundException(int treeId) 
        : base($"Tree with ID {treeId} was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to delete a node that has children.
/// </summary>
public class HasChildrenException : SecureException
{
    public HasChildrenException() 
        : base("You have to delete all children nodes first")
    {
    }

    public HasChildrenException(long nodeId) 
        : base($"Cannot delete node {nodeId} because it has children. You have to delete all children nodes first")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a circular reference in the tree.
/// </summary>
public class CircularReferenceException : SecureException
{
    public CircularReferenceException() 
        : base("Cannot create circular reference in tree structure.")
    {
    }

    public CircularReferenceException(long nodeId, long parentId) 
        : base($"Cannot set node {parentId} as parent of node {nodeId} - this would create a circular reference.")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a node with a parent from a different tree.
/// </summary>
public class InvalidParentTreeException : SecureException
{
    public InvalidParentTreeException() 
        : base("Parent node must belong to the same tree.")
    {
    }

    public InvalidParentTreeException(int treeId, int parentTreeId) 
        : base($"Cannot create node in tree {treeId} with parent from tree {parentTreeId}. Parent node must belong to the same tree.")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to set an invalid parent for a node.
/// </summary>
public class InvalidParentException : SecureException
{
    public InvalidParentException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails for tree operations.
/// </summary>
public class TreeValidationException : SecureException
{
    public TreeValidationException(string message) : base(message)
    {
    }
}