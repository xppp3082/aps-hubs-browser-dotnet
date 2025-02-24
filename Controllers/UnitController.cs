using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 管理戶型相關的 API 端點
/// </summary>
/// <remarks>
/// 這個控制器提供了所有與戶型相關的操作，包括：
/// - 獲取所有戶型
/// - 新增戶型
/// - 更新戶型
/// - 刪除戶型
/// - 分頁查詢戶型
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class UnitController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUnits()
    {
        var units = await _unitService.GetAllUnitsAsync();
        return Ok(units);
    }

    [HttpPost]
    public async Task<IActionResult> AddUnit([FromQuery] int projectId, [FromBody] Unit unit)
    {
        unit.ProjectId = projectId;
        var newUnit = await _unitService.AddUnitAsync(unit);
        return CreatedAtAction(nameof(GetAllUnits), new { id = newUnit.Id }, newUnit);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUnit([FromQuery] int id)
    {
        var result = await _unitService.DeleteUnitAsync(id);
        if (result)
        {
            return Ok("Unit deleted successfully");
        }
        return NotFound("Unit not found");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUnit([FromQuery] int id, [FromBody] Unit unit)
    {
        unit.Id = id;
        var updatedUnit = await _unitService.UpdateUnitAsync(unit);
        return Ok(updatedUnit);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetUnitsByProjectId([FromRoute] int projectId)
    {
        var units = await _unitService.GetUnitsByProjectIdAsync(projectId);
        return Ok(units);
    }

    /// <summary>
    /// 根據專案及代表客戶，獲取分頁的單位數據
    /// </summary>
    /// <param name="projectId"> 項目 ID</param>
    /// <param name="customerId"> 客戶 ID</param>
    /// <param name="page"> 頁碼 </param>
    /// <returns> 分頁的單位數據</returns>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedUnitsByProjectIdAndCustomerId(
        [FromQuery] int projectId,
        [FromQuery] int customerId,
        [FromQuery] int page = 1
    )
    {
        try
        {
            var result = await _unitService.GetPagedUnitsByProjectIdAndCustomerIdAsync(
                projectId,
                customerId,
                page
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
