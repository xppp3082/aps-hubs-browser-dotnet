using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

/// <summary>
/// 管理客變紀錄相關的 API 端點
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
public class ChangeDetailController : ControllerBase
{
    private readonly IChangeDetailService _changeDetailService;

    public ChangeDetailController(IChangeDetailService changeDetailService)
    {
        _changeDetailService = changeDetailService;
    }

    /// <summary>
    /// 獲取所有客變紀錄
    /// </summary>
    /// <returns> 所有客變紀錄</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllChangeDetails()
    {
        try
        {
            var changeDetails = await _changeDetailService.GetAllChangeDetailsAsync();
            return Ok(changeDetails);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetAllChangeDetails 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 依 Id 獲取客變紀錄
    /// </summary>
    /// <param name="id"> 客變紀錄 Id</param>
    /// <returns> 依 Id 獲取該筆客變紀錄</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChangeDetailById([FromRoute] int id)
    {
        try
        {
            var changeDetail = await _changeDetailService.GetChangeDetailByIdAsync(id);
            return Ok(changeDetail);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetChangeDetailById 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 依單位 Id 獲取客變紀錄
    /// </summary>
    /// <param name="unitId"> 單位 Id</param>
    /// <returns> 依單位 Id 獲取該單位所有客變紀錄</returns>
    [HttpGet("unit")]
    public async Task<IActionResult> GetChangeDetailsByUnitId([FromQuery] int unitId)
    {
        try
        {
            var changeDetails = await _changeDetailService.GetChangeDetailsByUnitIdAsync(unitId);
            return Ok(changeDetails);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetChangeDetailsByUnitId 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 依單位 Id 獲取分頁客變紀錄
    /// </summary>
    /// <param name="unitId"> 單位 Id</param>
    /// <param name="page"> 頁碼</param>
    /// <returns> 依單位 Id 獲取該單位所有客變紀錄</returns>
    [HttpGet("unit/paged")]
    public async Task<IActionResult> GetPagedChangeDetailsByUnitId(
        [FromQuery] int unitId,
        [FromQuery] int page = 1
    )
    {
        try
        {
            var changeDetails = await _changeDetailService.GetPagedChangeDetailsByUnitIdAsync(
                unitId,
                page
            );
            return Ok(changeDetails);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetPagedChangeDetailsByUnitId 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 新增客變紀錄
    /// </summary>
    /// <param name="changeDetail"> 客變紀錄</param>
    /// <returns> 新增的客變紀錄</returns>
    [HttpPost]
    public async Task<IActionResult> CreateChangeDetail([FromBody] ChangeDetail changeDetail)
    {
        try
        {
            var createdChangeDetail = await _changeDetailService.CreateChangeDetailAsync(changeDetail);
            return Ok(createdChangeDetail);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateChangeDetail 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新客變紀錄
    /// </summary>
    /// <param name="id"> 客變紀錄 Id</param>
    /// <param name="changeDetail"> 客變紀錄</param>
    /// <returns> 更新的客變紀錄</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChangeDetail(
        [FromRoute] int id,
        [FromBody] ChangeDetail changeDetail
    )
    {
        try
        {
            changeDetail.Id = id;
            var updatedChangeDetail = await _changeDetailService.UpdateChangeDetailAsync(changeDetail);
            return Ok(updatedChangeDetail);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UpdateChangeDetail 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 刪除客變紀錄
    /// </summary>
    /// <param name="id"> 客變紀錄 Id</param>
    /// <returns> 刪除的客變紀錄</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChangeDetail([FromRoute] int id)
    {
        try
        {
            var deleted = await _changeDetailService.DeleteChangeDetailAsync(id);
            return Ok(deleted);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DeleteChangeDetail 發生錯誤: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
