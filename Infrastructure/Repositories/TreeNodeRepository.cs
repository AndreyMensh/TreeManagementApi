using Microsoft.EntityFrameworkCore;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Domain.Entities;
using TreeManagementApi.Infrastructure.Data;

namespace TreeManagementApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TreeNode operations
/// </summary>
public class TreeNodeRepository : ITreeNodeRepository
{
    private readonly TreeManagementDbContext _context;

    public TreeNodeRepository(TreeManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TreeNode>> GetTreeAsync(int treeId)
    {
        return await _context.TreeNodes
            .Where(n => n.TreeId == treeId)
            .OrderBy(n => n.Path)
            .ToListAsync();
    }

    public async Task<TreeNode?> GetNodeAsync(int treeId, long nodeId)
    {
        return await _context.TreeNodes
            .Include(n => n.Parent)
            .Include(n => n.Children)
            .FirstOrDefaultAsync(n => n.TreeId == treeId && n.Id == nodeId);
    }

    public async Task<IEnumerable<TreeNode>> GetRootNodesAsync()
    {
        return await _context.TreeNodes
            .Where(n => n.ParentId == null)
            .OrderBy(n => n.TreeId)
            .ThenBy(n => n.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<TreeNode>> GetRootNodesAsync(int treeId)
    {
        return await _context.TreeNodes
            .Where(n => n.TreeId == treeId && n.ParentId == null)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<TreeNode>> GetChildrenAsync(int treeId, long parentId)
    {
        return await _context.TreeNodes
            .Where(n => n.TreeId == treeId && n.ParentId == parentId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<TreeNode>> GetSubtreeAsync(int treeId, long nodeId)
    {
        var rootNode = await GetNodeAsync(treeId, nodeId);
        if (rootNode?.Path == null)
            return new List<TreeNode>();

        return await _context.TreeNodes
            .Where(n => n.TreeId == treeId && n.Path!.StartsWith(rootNode.Path))
            .OrderBy(n => n.Path)
            .ToListAsync();
    }

    public async Task<bool> HasChildrenAsync(int treeId, long nodeId)
    {
        return await _context.TreeNodes
            .AnyAsync(n => n.TreeId == treeId && n.ParentId == nodeId);
    }

    public async Task<bool> TreeExistsAsync(int treeId)
    {
        return await _context.TreeNodes
            .AnyAsync(n => n.TreeId == treeId);
    }

    public async Task<IEnumerable<int>> GetAllTreeIdsAsync()
    {
        return await _context.TreeNodes
            .Select(n => n.TreeId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync();
    }

    public async Task<int> GetNextTreeIdAsync()
    {
        var maxTreeId = await _context.TreeNodes
            .Select(n => (int?)n.TreeId)
            .MaxAsync();
        
        return (maxTreeId ?? 0) + 1;
    }

    public async Task<TreeNode> AddNodeAsync(TreeNode node)
    {
        _context.TreeNodes.Add(node);
        await _context.SaveChangesAsync();

        // Update the path after the node is saved and has an ID
        if (node.Parent == null)
        {
            node.Path = $"{node.Id}.";
        }
        else
        {
            node.Path = $"{node.Parent.Path}{node.Id}.";
        }

        await _context.SaveChangesAsync();
        return node;
    }

    public async Task<TreeNode> UpdateNodeAsync(TreeNode node)
    {
        _context.TreeNodes.Update(node);
        await _context.SaveChangesAsync();
        return node;
    }

    public async Task DeleteNodeAsync(TreeNode node)
    {
        _context.TreeNodes.Remove(node);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePathsAsync(TreeNode node)
    {
        // Update the node's path
        node.UpdatePath();

        // Get all descendants and update their paths
        var descendants = await _context.TreeNodes
            .Where(n => n.TreeId == node.TreeId && n.Path!.StartsWith($"{node.Id}."))
            .ToListAsync();

        foreach (var descendant in descendants)
        {
            // Recalculate path by walking up the parent chain
            var path = new List<long>();
            var current = descendant;
            
            while (current != null)
            {
                path.Insert(0, current.Id);
                current = await _context.TreeNodes
                    .FirstOrDefaultAsync(n => n.Id == current.ParentId && n.TreeId == current.TreeId);
            }

            descendant.Path = string.Join(".", path) + ".";
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateParentAsync(int treeId, long parentId)
    {
        return await _context.TreeNodes
            .AnyAsync(n => n.TreeId == treeId && n.Id == parentId);
    }

    public async Task<bool> WouldCreateCircularReferenceAsync(int treeId, long nodeId, long parentId)
    {
        // Get the proposed parent node
        var parentNode = await GetNodeAsync(treeId, parentId);
        if (parentNode?.Path == null)
            return false;

        // Check if the proposed parent is a descendant of the current node
        var nodePathPrefix = $"{nodeId}.";
        return parentNode.Path.Contains(nodePathPrefix);
    }
}