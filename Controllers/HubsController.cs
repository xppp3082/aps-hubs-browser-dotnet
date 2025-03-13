using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.DataManagement.Model;
using Microsoft.AspNetCore.Mvc;

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
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
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
                    tipVersion = (object)null, // 添加一個空的 tipVersion 屬性以保持結構一致
                };

            var itemsList = new List<object>();
            foreach (var entry in contents.Where(e => e is ItemData))
            {
                var item = entry as ItemData;
                var tipVersion = await _aps.GetTipVersion(project, item.Id, tokens);

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
                        tipVersion = tipVersion != null
                            ? tipVersion.Attributes.Extension.Data
                            : null,
                    }
                );
            }
            // var items =
            //     from entry in contents
            //     where entry is ItemData
            //     select entry as ItemData into item
            //     select new
            //     {
            //         id = item.Id,
            //         name = item.Attributes.DisplayName,
            //         createdTime = item.Attributes.CreateTime,
            //         createUserName = item.Attributes.CreateUserName,
            //         lastModifiedTime = item.Attributes.LastModifiedTime,
            //         lastModifiedUserName = item.Attributes.LastModifiedUserName,
            //         folder = false,
            //         tipVersion = item.Relationships.Tip,
            //     };
            // return Ok(folders.Concat(items));
            var result = folders.Cast<object>().Concat(itemsList);
            return Ok(result);
        }
    }

    [HttpGet("{hub}/projects/{project}/contents/{item}/versions")]
    public async Task<ActionResult> ListVersions(string hub, string project, string item)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            return Unauthorized();
        }
        return Ok(
            from version in await _aps.GetVersions(project, item, tokens)
            select new { id = version.Id, name = version.Attributes.CreateTime }
        );
    }

    [HttpGet("{hub}/projects/{project}/contents/{item}/tip")]
    public async Task<ActionResult> GetTipVersion(string hub, string project, string item)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            return Unauthorized();
        }

        // Get the tip version directly
        var tipVersion = await _aps.GetTipVersion(project, item, tokens);

        if (tipVersion == null)
        {
            return NotFound(); // Handle case where no version is found
        }
        // return Ok(tipVersion);
        return Ok(new { id = tipVersion.Id, attributes = tipVersion.Attributes });
    }
}
