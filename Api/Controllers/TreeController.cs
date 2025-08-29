using Microsoft.AspNetCore.Mvc;
using TreeManagementApi.Application.DTOs;
using TreeManagementApi.Application.Interfaces;

namespace TreeManagementApi.Api.Controllers;

/// <summary>
/// REST API controller for tree management operations.
/// Provides endpoints for creating, reading, updating, and deleting tree structures.
/// </summary>
[ApiController]
[Route("api/tree")]
[Produces("application/json")]
public class TreeController : ControllerBase
{
    private readonly ITreeService _treeService;
    private readonly ILogger<TreeController> _logger;

    public TreeController(ITreeService treeService, ILogger<TreeController> logger)
    {
        _treeService = treeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available trees with summary information
    /// </summary>
    /// <returns>List of trees with basic information</returns>
    /// <response code="200">Returns the list of all trees</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TreeSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreeSummaryDto>>> GetAllTrees()
    {
        _logger.LogInformation("Getting all trees");
        var trees = await _treeService.GetAllTreesAsync();
        return Ok(trees);
    }

    /// <summary>
    /// Gets a complete tree structure by tree ID
    /// </summary>
    /// <param name="treeId">ID of the tree to retrieve</param>
    /// <returns>Complete tree structure with all nodes</returns>
    /// <response code="200">Returns the complete tree structure</response>
    /// <response code="404">Tree not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{treeId}")]
    [ProducesResponseType(typeof(TreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeDto>> GetTree(int treeId)
    {
        _logger.LogInformation("Getting tree with ID {TreeId}", treeId);
        var tree = await _treeService.GetTreeAsync(treeId);
        return Ok(tree);
    }

    /// <summary>
    /// Creates a new tree with a root node
    /// </summary>
    /// <param name="request">Request containing the root node name</param>
    /// <returns>The created tree information</returns>
    /// <response code="201">Tree created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(TreeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeDto>> CreateTree([FromBody] CreateTreeRequest request)
    {
        _logger.LogInformation("Creating new tree with name '{TreeName}'", request.TreeName);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tree = await _treeService.CreateTreeAsync(request);
        return CreatedAtAction(nameof(GetTree), new { treeId = tree.TreeId }, tree);
    }

    /// <summary>
    /// Creates a new node in an existing tree
    /// </summary>
    /// <param name="treeId">ID of the tree to add the node to</param>
    /// <param name="request">Request containing parent ID and node name</param>
    /// <returns>The created node information</returns>
    /// <response code="201">Node created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Tree or parent node not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{treeId}/node")]
    [ProducesResponseType(typeof(TreeNodeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeNodeDto>> CreateNode(int treeId, [FromBody] CreateNodeRequest request)
    {
        _logger.LogInformation("Creating new node '{NodeName}' in tree {TreeId} with parent {ParentId}", 
            request.NodeName, treeId, request.ParentId);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var node = await _treeService.CreateNodeAsync(treeId, request);
        return CreatedAtAction(nameof(GetNode), new { treeId = treeId, nodeId = node.Id }, node);
    }

    /// <summary>
    /// Deletes a node from a tree. The node must not have any children.
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to delete</param>
    /// <response code="200">Node deleted successfully</response>
    /// <response code="404">Tree or node not found</response>
    /// <response code="500">Internal server error (including HasChildrenException)</response>
    [HttpDelete("{treeId}/node/{nodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteNode(int treeId, long nodeId)
    {
        _logger.LogInformation("Deleting node {NodeId} from tree {TreeId}", nodeId, treeId);
        
        await _treeService.DeleteNodeAsync(treeId, nodeId);
        return Ok(new { message = "Node deleted successfully" });
    }

    /// <summary>
    /// Renames a node in a tree
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to rename</param>
    /// <param name="request">Request containing the new name</param>
    /// <returns>The updated node information</returns>
    /// <response code="200">Node renamed successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Tree or node not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{treeId}/node/{nodeId}/rename")]
    [ProducesResponseType(typeof(TreeNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeNodeDto>> RenameNode(
        int treeId, 
        long nodeId, 
        [FromBody] RenameNodeRequest request)
    {
        _logger.LogInformation("Renaming node {NodeId} in tree {TreeId} to '{NewName}'", 
            nodeId, treeId, request.NewName);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var node = await _treeService.RenameNodeAsync(treeId, nodeId, request);
        return Ok(node);
    }

    /// <summary>
    /// Gets a specific node by ID
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the node to retrieve</param>
    /// <returns>Node information</returns>
    /// <response code="200">Returns the node information</response>
    /// <response code="404">Tree or node not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{treeId}/node/{nodeId}")]
    [ProducesResponseType(typeof(TreeNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeNodeDto>> GetNode(int treeId, long nodeId)
    {
        _logger.LogInformation("Getting node {NodeId} from tree {TreeId}", nodeId, treeId);
        var node = await _treeService.GetNodeAsync(treeId, nodeId);
        return Ok(node);
    }

    /// <summary>
    /// Gets all children of a specific node
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the parent node</param>
    /// <returns>List of child nodes</returns>
    /// <response code="200">Returns the list of child nodes</response>
    /// <response code="404">Tree or node not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{treeId}/node/{nodeId}/children")]
    [ProducesResponseType(typeof(IEnumerable<TreeNodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreeNodeDto>>> GetNodeChildren(int treeId, long nodeId)
    {
        _logger.LogInformation("Getting children of node {NodeId} from tree {TreeId}", nodeId, treeId);
        var children = await _treeService.GetNodeChildrenAsync(treeId, nodeId);
        return Ok(children);
    }

    /// <summary>
    /// Gets the complete subtree starting from a specific node
    /// </summary>
    /// <param name="treeId">ID of the tree containing the node</param>
    /// <param name="nodeId">ID of the root node of the subtree</param>
    /// <returns>Complete subtree structure</returns>
    /// <response code="200">Returns the complete subtree structure</response>
    /// <response code="404">Tree or node not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{treeId}/node/{nodeId}/subtree")]
    [ProducesResponseType(typeof(TreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreeDto>> GetSubtree(int treeId, long nodeId)
    {
        _logger.LogInformation("Getting subtree starting from node {NodeId} in tree {TreeId}", nodeId, treeId);
        var subtree = await _treeService.GetSubtreeAsync(treeId, nodeId);
        return Ok(subtree);
    }
}