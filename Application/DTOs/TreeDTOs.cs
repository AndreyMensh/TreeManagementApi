using System.ComponentModel.DataAnnotations;

namespace TreeManagementApi.Application.DTOs;

/// <summary>
/// DTO for creating a new tree (root node)
/// </summary>
public class CreateTreeRequest
{
    /// <summary>
    /// Name of the root node of the new tree
    /// </summary>
    [Required(ErrorMessage = "Tree name is required")]
    [StringLength(255, ErrorMessage = "Tree name cannot exceed 255 characters")]
    public string TreeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new node in an existing tree
/// </summary>
public class CreateNodeRequest
{
    /// <summary>
    /// ID of the parent node. Must exist in the same tree.
    /// </summary>
    [Required(ErrorMessage = "Parent ID is required")]
    public long ParentId { get; set; }

    /// <summary>
    /// Name of the new node
    /// </summary>
    [Required(ErrorMessage = "Node name is required")]
    [StringLength(255, ErrorMessage = "Node name cannot exceed 255 characters")]
    public string NodeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for renaming a node
/// </summary>
public class RenameNodeRequest
{
    /// <summary>
    /// New name for the node
    /// </summary>
    [Required(ErrorMessage = "New name is required")]
    [StringLength(255, ErrorMessage = "Node name cannot exceed 255 characters")]
    public string NewName { get; set; } = string.Empty;
}

/// <summary>
/// DTO representing a tree node in API responses
/// </summary>
public class TreeNodeDto
{
    /// <summary>
    /// Unique identifier of the node
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Name of the node
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tree ID this node belongs to
    /// </summary>
    public int TreeId { get; set; }

    /// <summary>
    /// Parent node ID (null for root nodes)
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Materialized path for the node
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Level in the tree hierarchy (0 for root)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Indicates if this is a root node
    /// </summary>
    public bool IsRoot { get; set; }

    /// <summary>
    /// Child nodes (if included in response)
    /// </summary>
    public List<TreeNodeDto>? Children { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for tree summary information
/// </summary>
public class TreeSummaryDto
{
    /// <summary>
    /// Tree identifier
    /// </summary>
    public int TreeId { get; set; }

    /// <summary>
    /// Name of the root node
    /// </summary>
    public string RootName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of nodes in the tree
    /// </summary>
    public int NodeCount { get; set; }

    /// <summary>
    /// Maximum depth of the tree
    /// </summary>
    public int MaxDepth { get; set; }

    /// <summary>
    /// Creation date of the tree (root node creation date)
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for the complete tree structure
/// </summary>
public class TreeDto
{
    /// <summary>
    /// Tree identifier
    /// </summary>
    public int TreeId { get; set; }

    /// <summary>
    /// Root nodes of the tree (usually just one, but the structure supports multiple roots)
    /// </summary>
    public List<TreeNodeDto> Nodes { get; set; } = new();

    /// <summary>
    /// Total number of nodes in the tree
    /// </summary>
    public int TotalNodes { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for error responses
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Type of error ("Secure" for SecureException, "Exception" for other exceptions)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Event ID for tracking the exception in logs
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Error data containing the message
    /// </summary>
    public ErrorDataDto Data { get; set; } = new();
}

/// <summary>
/// DTO for error data within error responses
/// </summary>
public class ErrorDataDto
{
    /// <summary>
    /// Error message to display to the user
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO for successful operation responses
/// </summary>
public class SuccessResponseDto<T>
{
    /// <summary>
    /// Indicates operation was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Optional message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// DTO for paginated responses
/// </summary>
public class PagedResponseDto<T>
{
    /// <summary>
    /// List of items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}