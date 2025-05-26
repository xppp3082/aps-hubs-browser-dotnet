using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.DataManagement.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[ApiController]
[Route("api/[controller]")]
public class HubsController : ControllerBase
{
    private readonly APS _aps;

    public HubsController(APS aps)
    {
        _aps = aps;
    }

    [HttpGet()]
    public async Task<ActionResult> ListHubs()
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            Log.Warning("ListHubs 未授權");
            return Unauthorized();
        }
        return Ok(
            from hub in await _aps.GetHubs(tokens)
            select new { id = hub.Id, name = hub.Attributes.Name }
        );
    }

    [HttpGet("{hub}/projects")]
    public async Task<ActionResult> ListProjects(string hub)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            Log.Warning("ListProjects 未授權");
            return Unauthorized();
        }
        return Ok(
            from project in await _aps.GetProjects(hub, tokens)
            select new { id = project.Id, name = project.Attributes.Name }
        );
    }

    [HttpGet("{hub}/projects/{project}/contents")]
    public async Task<ActionResult> ListItems(
        string hub,
        string project,
        [FromQuery] string folder_id
    )
    {
        try
        {
            var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
            if (tokens == null)
            {
                Log.Warning("ListItems 未授權");
                return Unauthorized();
            }
            if (string.IsNullOrEmpty(folder_id))
            {
                return Ok(
                    from folder in await _aps.GetTopFolders(hub, project, tokens)
                    select new
                    {
                        id = folder.Id,
                        name = folder.Attributes.DisplayName,
                        folder = true,
                    }
                );
            }
            else
            {
                var contents = await _aps.GetFolderContents(project, folder_id, tokens);
                var folders =
                    from entry in contents
                    where entry is FolderData
                    select entry as FolderData into folder
                    select new
                    {
                        id = folder.Id,
                        name = folder.Attributes.DisplayName,
                        createdTime = folder.Attributes.CreateTime,
                        createUserName = folder.Attributes.CreateUserName,
                        lastModifiedTime = folder.Attributes.LastModifiedTime,
                        lastModifiedUserName = folder.Attributes.LastModifiedUserName,
                        folder = true,
                    };

                var itemsList = new List<object>();
                foreach (var entry in contents.Where(e => e is ItemData))
                {
                    var item = entry as ItemData;

                    itemsList.Add(
                        new
                        {
                            id = item.Id,
                            name = item.Attributes.DisplayName,
                            createdTime = item.Attributes.CreateTime,
                            createUserName = item.Attributes.CreateUserName,
                            lastModifiedTime = item.Attributes.LastModifiedTime,
                            lastModifiedUserName = item.Attributes.LastModifiedUserName,
                            folder = false,
                        }
                    );
                }
                var result = folders.Cast<object>().Concat(itemsList);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ListItems 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{hub}/projects/{project}/full-contents")]
    public async Task<ActionResult> ListFullContent(
        string hub,
        string project,
        [FromQuery] string folder_id
    )
    {
        try
        {
            var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
            if (tokens == null)
            {
                Log.Warning("ListFullContent 未授權");
                return Unauthorized();
            }

            var contents = await _aps.GetFolderContents(project, folder_id, tokens);
            var result = new List<object>();

            foreach (var entry in contents)
            {
                if (entry is FolderData folder)
                {
                    result.Add(new
                    {
                        id = folder.Id,
                        name = folder.Attributes.DisplayName,
                        createdTime = folder.Attributes.CreateTime,
                        createUserName = folder.Attributes.CreateUserName,
                        lastModifiedTime = folder.Attributes.LastModifiedTime,
                        lastModifiedUserName = folder.Attributes.LastModifiedUserName,
                        folder = true,
                        items = await GetFolderContentsRecursively(project, folder.Id, tokens)
                    });
                }
                else if (entry is ItemData item)
                {
                    result.Add(new
                    {
                        id = item.Id,
                        name = item.Attributes.DisplayName,
                        createdTime = item.Attributes.CreateTime,
                        createUserName = item.Attributes.CreateUserName,
                        lastModifiedTime = item.Attributes.LastModifiedTime,
                        lastModifiedUserName = item.Attributes.LastModifiedUserName,
                        folder = false
                    });
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ListFullContent 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<List<object>> GetFolderContentsRecursively(
        string project,
        string folderId,
        Tokens tokens
    )
    {
        var contents = await _aps.GetFolderContents(project, folderId, tokens);
        var result = new List<object>();

        foreach (var entry in contents)
        {
            if (entry is FolderData folder)
            {
                result.Add(new
                {
                    id = folder.Id,
                    name = folder.Attributes.DisplayName,
                    createdTime = folder.Attributes.CreateTime,
                    createUserName = folder.Attributes.CreateUserName,
                    lastModifiedTime = folder.Attributes.LastModifiedTime,
                    lastModifiedUserName = folder.Attributes.LastModifiedUserName,
                    folder = true,
                    items = await GetFolderContentsRecursively(project, folder.Id, tokens)
                });
            }
            else if (entry is ItemData item)
            {
                result.Add(new
                {
                    id = item.Id,
                    name = item.Attributes.DisplayName,
                    createdTime = item.Attributes.CreateTime,
                    createUserName = item.Attributes.CreateUserName,
                    lastModifiedTime = item.Attributes.LastModifiedTime,
                    lastModifiedUserName = item.Attributes.LastModifiedUserName,
                    folder = false
                });
            }
        }

        return result;
    }

    [HttpGet("{hub}/projects/{project}/contents/{item}/urn")]
    public async Task<ActionResult> GetFileUrn(string hub, string project, string item)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            Log.Warning("GetFileUrn 未授權");
            return Unauthorized();
        }

        try
        {
            var tipVersion = await _aps.GetTipVersion(project, item, tokens);
            if (tipVersion == null)
            {
                Log.Warning("GetFileUrn: Tip version not found");
                return NotFound("Tip version not found");
            }

            var urn = tipVersion.Relationships?.Storage?.Data?.Id;
            if (string.IsNullOrEmpty(urn))
            {
                Log.Warning("GetFileUrn: URN not found");
                return NotFound("URN not found");
            }

            return Ok(new { urn });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetFileUrn 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{hub}/projects/{project}/folder-tree")]
    public async Task<ActionResult> GetFolderTree(string hub, string project)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            Log.Warning("GetFolderTree 未授權");
            return Unauthorized();
        }

        try
        {
            var topFolders = await _aps.GetTopFolders(hub, project, tokens);
            var result = new List<object>();

            foreach (var folder in topFolders)
            {
                var folderContents = await _aps.GetFolderContents(project, folder.Id, tokens);
                var subFolders = folderContents.Where(item => item is FolderData)
                    .Select(item => item as FolderData)
                    .Select(subFolder => new
                    {
                        id = subFolder.Id,
                        name = subFolder.Attributes.DisplayName,
                        folder = true
                    });

                result.Add(new
                {
                    id = folder.Id,
                    name = folder.Attributes.DisplayName,
                    folder = true,
                    items = subFolders
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetFolderTree 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
