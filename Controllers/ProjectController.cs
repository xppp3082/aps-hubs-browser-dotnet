using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

/// <summary>
/// 管理項目相關的 API 端點
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 查詢項目是否存在，不存在則新增
    /// </summary>
    /// <param name="projectInfo">項目資訊</param>
    /// <returns>項目資訊</returns>
    [HttpPost]
    public async Task<IActionResult> CheckOrCreateProject([FromBody] ProjectInfo projectInfo)
    {
        try
        {
            var result = await _projectService.CheckOrCreateProjectAsync(projectInfo);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CheckOrCreateProject 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
