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
                    // tipVersion = (object)null, // 添加一個空的 tipVersion 屬性以保持結構一致
                };

            var itemsList = new List<object>();
            foreach (var entry in contents.Where(e => e is ItemData))
            {
                var item = entry as ItemData;
                // var tipVersion = await _aps.GetTipVersion(project, item.Id, tokens);

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
                        // tipVersion = tipVersion != null
                        //     ? tipVersion.Attributes.Extension.Data
                        //     : null,
                    }
                );
            }
            var result = folders.Cast<object>().Concat(itemsList);
            return Ok(result);
        }
    }

    [HttpGet("{hub}/projects/{project}/full-contents")]
    public async Task<ActionResult> ListFullContent(
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
            var contents = await _aps.GetFullFolderContents(project, folder_id, tokens);
            var mainData = contents.Data;
            var includedData = contents.Included;

            // 處理資料夾
            var folders =
                from entry in mainData
                where entry is FolderData
                select entry as FolderData into folder
                select new
                {
                    id = folder.Id,
                    versionId = (object)null,
                    name = folder.Attributes.DisplayName,
                    createdTime = folder.Attributes.CreateTime,
                    createUserName = folder.Attributes.CreateUserName,
                    lastModifiedTime = folder.Attributes.LastModifiedTime,
                    lastModifiedUserName = folder.Attributes.LastModifiedUserName,
                    folder = true,
                    tipVersion = (object)null, // 添加一個空的 tipVersion 屬性以保持結構一致
                };

            // 處理項目及其版本
            var items = new List<object>();
            foreach (var entry in mainData.Where(e => e is ItemData))
            {
                var item = entry as ItemData;
                object versionData = null;

                VersionData versionInfo = null;
                // 從 item 的關係中獲取 tip 版本的 ID
                if (item.Relationships?.Tip?.Data?.Id != null)
                {
                    var versionId = item.Relationships.Tip.Data.Id;
                    // 從 includedData 中查找對應的版本信息
                    versionInfo = includedData?.FirstOrDefault(i => i.Id.Equals(versionId));

                    if (versionInfo != null && versionInfo.Attributes != null)
                    {
                        // 創建一個包含所需版本信息的對象
                        versionData = new
                        {
                            versionNumber = versionInfo.Attributes.VersionNumber,
                            processState = versionInfo.Attributes.Extension?.Data != null
                            && versionInfo.Attributes.Extension.Data.ContainsKey("processState")
                                ? versionInfo.Attributes.Extension.Data["processState"]
                                : null,
                            extractionState = versionInfo.Attributes.Extension?.Data != null
                            && versionInfo.Attributes.Extension.Data.ContainsKey("extractionState")
                                ? versionInfo.Attributes.Extension.Data["extractionState"]
                                : null,
                            revisionDisplayLabel = versionInfo.Attributes.Extension?.Data != null
                            && versionInfo.Attributes.Extension.Data.ContainsKey(
                                "revisionDisplayLabel"
                            )
                                ? versionInfo.Attributes.Extension.Data["revisionDisplayLabel"]
                                : null,
                        };
                    }
                }

                items.Add(
                    new
                    {
                        id = item.Id,
                        versionId = versionInfo.Id,
                        name = item.Attributes.DisplayName,
                        createdTime = item.Attributes.CreateTime,
                        createUserName = item.Attributes.CreateUserName,
                        lastModifiedTime = item.Attributes.LastModifiedTime,
                        lastModifiedUserName = item.Attributes.LastModifiedUserName,
                        folder = false,
                        tipVersion = versionData,
                    }
                );
            }

            // 合併結果
            var result = folders.Cast<object>().Concat(items);
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

    [HttpGet("{hub}/projects/{project}/contents/{item}/urn")]
    public async Task<ActionResult> GetFileUrn(string hub, string project, string item)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            return Unauthorized();
        }

        try
        {
            // Get the tip version of the item
            var tipVersion = await _aps.GetTipVersion(project, item, tokens);
            if (tipVersion == null)
            {
                return NotFound("Tip version not found");
            }

            // Get the URN from the tip version's relationships
            var urn = tipVersion.Relationships?.Storage?.Data?.Id;
            if (string.IsNullOrEmpty(urn))
            {
                return NotFound("URN not found");
            }

            return Ok(new { urn });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{hub}/projects/{project}/folder-tree")]
    public async Task<ActionResult> GetFolderTree(string hub, string project)
    {
        var tokens = await AuthController.PrepareTokens(Request, Response, _aps);
        if (tokens == null)
        {
            return Unauthorized();
        }

        try
        {
            // 首先獲取頂層資料夾（Project Files）
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
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
