using TreeManagementApi.Application.DTOs;
using TreeManagementApi.Application.Exceptions;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Domain.Entities;

namespace TreeManagementApi.Application.Services;

/// <summary>
/// Service implementation for tree management operations.
/// Contains all business logic for tree operations.
/// </summary>
public class TreeService : ITreeService
{
    private readonly IUnitOfWork _unitOfWork;

    public TreeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TreeSummaryDto>> GetAllTreesAsync()
    {
        var treeIds = await _unitOfWork.TreeNodes.GetAllTreeIdsAsync();
        var treeSummaries = new List<TreeSummaryDto>();

        foreach (var treeId in treeIds)
        {
            var rootNodes = await _unitOfWork.TreeNodes.GetRootNodesAsync(treeId);
            var rootNode = rootNodes.FirstOrDefault();

            if (rootNode != null)
            {
                var allNodes = await _unitOfWork.TreeNodes.GetTreeAsync(treeId);
                var nodesList = allNodes.ToList();

                treeSummaries.Add(new TreeSummaryDto
                {
                    TreeId = treeId,
                    RootName = rootNode.Name,
                    NodeCount = nodesList.Count,
                    MaxDepth = nodesList.Any() ? nodesList.Max(n => n.Level) : 0,
                    CreatedAt = rootNode.CreatedAt
                });
            }
        }

        return treeSummaries.OrderBy(t => t.TreeId);
    }

    public async Task<TreeDto> GetTreeAsync(int treeId)
    {
        if (!await _unitOfWork.TreeNodes.TreeExistsAsync(treeId))
        {
            throw new TreeNotFoundException(treeId);
        }

        var nodes = await _unitOfWork.TreeNodes.GetTreeAsync(treeId);
        var nodesList = nodes.ToList();

        return new TreeDto
        {
            TreeId = treeId,
            Nodes = MapToTreeNodeDtos(nodesList),
            TotalNodes = nodesList.Count,
            CreatedAt = nodesList.Any() ? nodesList.Min(n => n.CreatedAt) : DateTime.UtcNow
        };
    }

    public async Task<TreeDto> CreateTreeAsync(CreateTreeRequest request)
    {
        var nextTreeId = await _unitOfWork.TreeNodes.GetNextTreeIdAsync();

        var rootNode = new TreeNode
        {
            Name = request.TreeName,
            TreeId = nextTreeId,
            ParentId = null,
            CreatedAt = DateTime.UtcNow
        };

        var createdNode = await _unitOfWork.TreeNodes.AddNodeAsync(rootNode);

        return new TreeDto
        {
            TreeId = nextTreeId,
            Nodes = new List<TreeNodeDto> { MapToTreeNodeDto(createdNode) },
            TotalNodes = 1,
            CreatedAt = createdNode.CreatedAt
        };
    }

    public async Task<TreeNodeDto> CreateNodeAsync(int treeId, CreateNodeRequest request)
    {
        // Validate tree exists
        if (!await _unitOfWork.TreeNodes.TreeExistsAsync(treeId))
        {
            throw new TreeNotFoundException(treeId);
        }

        // Validate parent exists and belongs to the same tree
        if (!await _unitOfWork.TreeNodes.ValidateParentAsync(treeId, request.ParentId))
        {
            throw new NodeNotFoundException(request.ParentId, treeId);
        }

        // Get parent node to set up the path
        var parent = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, request.ParentId);
        if (parent == null)
        {
            throw new NodeNotFoundException(request.ParentId, treeId);
        }

        var newNode = new TreeNode
        {
            Name = request.NodeName,
            TreeId = treeId,
            ParentId = request.ParentId,
            Parent = parent,
            CreatedAt = DateTime.UtcNow
        };

        var createdNode = await _unitOfWork.TreeNodes.AddNodeAsync(newNode);
        return MapToTreeNodeDto(createdNode);
    }

    public async Task DeleteNodeAsync(int treeId, long nodeId)
    {
        var node = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, nodeId);
        if (node == null)
        {
            throw new NodeNotFoundException(nodeId, treeId);
        }

        // Check if node has children
        if (await _unitOfWork.TreeNodes.HasChildrenAsync(treeId, nodeId))
        {
            throw new HasChildrenException(nodeId);
        }

        await _unitOfWork.TreeNodes.DeleteNodeAsync(node);
    }

    public async Task<TreeNodeDto> RenameNodeAsync(int treeId, long nodeId, RenameNodeRequest request)
    {
        var node = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, nodeId);
        if (node == null)
        {
            throw new NodeNotFoundException(nodeId, treeId);
        }

        node.Name = request.NewName;
        var updatedNode = await _unitOfWork.TreeNodes.UpdateNodeAsync(node);
        
        return MapToTreeNodeDto(updatedNode);
    }

    public async Task<TreeNodeDto> GetNodeAsync(int treeId, long nodeId)
    {
        var node = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, nodeId);
        if (node == null)
        {
            throw new NodeNotFoundException(nodeId, treeId);
        }

        return MapToTreeNodeDto(node);
    }

    public async Task<IEnumerable<TreeNodeDto>> GetNodeChildrenAsync(int treeId, long nodeId)
    {
        // Verify node exists
        var node = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, nodeId);
        if (node == null)
        {
            throw new NodeNotFoundException(nodeId, treeId);
        }

        var children = await _unitOfWork.TreeNodes.GetChildrenAsync(treeId, nodeId);
        return children.Select(MapToTreeNodeDto);
    }

    public async Task<TreeDto> GetSubtreeAsync(int treeId, long nodeId)
    {
        // Verify node exists
        var rootNode = await _unitOfWork.TreeNodes.GetNodeAsync(treeId, nodeId);
        if (rootNode == null)
        {
            throw new NodeNotFoundException(nodeId, treeId);
        }

        var subtreeNodes = await _unitOfWork.TreeNodes.GetSubtreeAsync(treeId, nodeId);
        var nodesList = subtreeNodes.ToList();

        return new TreeDto
        {
            TreeId = treeId,
            Nodes = MapToTreeNodeDtos(nodesList),
            TotalNodes = nodesList.Count,
            CreatedAt = rootNode.CreatedAt
        };
    }

    /// <summary>
    /// Maps a TreeNode entity to a TreeNodeDto
    /// </summary>
    private static TreeNodeDto MapToTreeNodeDto(TreeNode node)
    {
        return new TreeNodeDto
        {
            Id = node.Id,
            Name = node.Name,
            TreeId = node.TreeId,
            ParentId = node.ParentId,
            Path = node.Path,
            Level = node.Level,
            IsRoot = node.IsRoot,
            CreatedAt = node.CreatedAt
        };
    }

    /// <summary>
    /// Maps a collection of TreeNode entities to TreeNodeDtos with hierarchical structure
    /// </summary>
    private static List<TreeNodeDto> MapToTreeNodeDtos(IEnumerable<TreeNode> nodes)
    {
        var nodesList = nodes.ToList();
        var dtoMap = nodesList.ToDictionary(n => n.Id, MapToTreeNodeDto);

        // Build hierarchical structure
        foreach (var node in nodesList)
        {
            var dto = dtoMap[node.Id];
            dto.Children = nodesList
                .Where(n => n.ParentId == node.Id)
                .Select(n => dtoMap[n.Id])
                .OrderBy(d => d.Id)
                .ToList();
        }

        // Return only root nodes (the children are nested within them)
        return dtoMap.Values
            .Where(d => d.IsRoot)
            .OrderBy(d => d.Id)
            .ToList();
    }
}