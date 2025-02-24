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

    /// <summary>
    /// 獲取所有戶型
    /// </summary>
    /// <returns> 所有戶型</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllUnits()
    {
        var units = await _unitService.GetAllUnitsAsync();
        return Ok(units);
    }

    /// <summary>
    /// 新增戶型
    /// </summary>
    /// <param name="projectId"> 項目 Id</param>
    /// <param name="customerId"> 客戶 Id</param>
    /// <param name="unit"> 戶型</param>
    /// <returns> 新增的戶型</returns>
    [HttpPost]
    public async Task<IActionResult> AddUnit(
        [FromQuery] int projectId,
        [FromQuery] int customerId,
        [FromBody] Unit unit
    )
    {
        // unit.ProjectId = projectId;
        var newUnit = await _unitService.AddUnitAsync(unit, customerId, projectId);
        return CreatedAtAction(nameof(GetAllUnits), new { id = newUnit.Id }, newUnit);
    }

    /// <summary>
    /// 刪除戶型
    /// </summary>
    /// <param name="id"> 戶型 Id</param>
    /// <returns> 刪除的戶型</returns>
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

    /// <summary>
    /// 更新戶型
    /// </summary>
    /// <param name="id"> 戶型 Id</param>
    /// <param name="projectId"> 項目 Id</param>
    /// <param name="customerId"> 客戶 Id</param>
    /// <param name="unit"> 戶型</param>
    /// <returns> 更新的戶型</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateUnit(
        [FromQuery] int id,
        [FromQuery] int projectId,
        [FromQuery] int customerId,
        [FromBody] Unit unit
    )
    {
        unit.Id = id;
        Console.WriteLine(unit);
        var updatedUnit = await _unitService.UpdateUnitAsync(unit, projectId, customerId);
        return Ok(updatedUnit);
    }

    // [HttpGet("project/{projectId}")]
    // public async Task<IActionResult> GetUnitsByProjectId([FromRoute] int projectId)
    // {
    //     var units = await _unitService.GetUnitsByProjectIdAsync(projectId);
    //     return Ok(units);
    // }

    /// <summary>
    /// 根據專案及代表客戶，獲取分頁的單位數據
    /// </summary>
    /// <param name="projectId"> 項目 ID</param>
    /// <param name="customerId"> 客戶 ID</param>
    /// <returns> 分頁的單位數據</returns>
    [HttpGet("project/{projectId}/customer/{customerId}")]
    public async Task<IActionResult> GetUnitsByCustomerProjectId(
        [FromRoute] int projectId,
        [FromRoute] int customerId
    )
    {
        var units = await _unitService.GetUnitsByCustomerProjectIdAsync(projectId, customerId);
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
