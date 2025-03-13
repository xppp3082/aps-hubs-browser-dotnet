using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// 獲取所有客戶
    /// </summary>
    /// <returns> 所有客戶</returns>
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();

        if (customers == null || customers.Count == 0)
        {
            return NotFound("No customers found.");
        }
        return Ok(customers);
    }

    /// <summary>
    /// 分頁獲取所有客戶
    /// </summary>
    /// <param name="page"> 頁碼</param>
    /// <returns> 分頁獲取所有客戶</returns>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<Customer>>> GetCustomers([FromQuery] int page = 1)
    {
        try
        {
            var result = await _customerService.GetPagedCustomersAsync(page);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // /// <summary>
    // /// 分頁獲取指定專案的客戶
    // /// </summary>
    // /// <param name="projectId">專案ID</param>
    // /// <param name="page">頁碼</param>
    // /// <returns>分頁獲取指定專案的客戶</returns>
    // [HttpGet("project/{projectId}/paged")]
    // public async Task<ActionResult<PagedResult<Customer>>> GetCustomersByProject(
    //     int projectId,
    //     [FromQuery] int page = 1
    // )
    // {
    //     try
    //     {
    //         var result = await _customerService.GetPagedCustomersByProjectAsync(projectId, page);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// 分頁獲取指定專案的客戶
    /// </summary>
    /// <param name="projectUrn">專案URN</param>
    /// <param name="page">頁碼</param>
    /// <returns>分頁獲取指定專案的客戶</returns>
    [HttpGet("project/{projectUrn}/paged")]
    public async Task<ActionResult<PagedResult<Customer>>> GetCustomersByProjectUrn(
        string projectUrn,
        [FromQuery] int page = 1
    )
    {
        try
        {
            var result = await _customerService.GetPagedCustomersByProjectUrnAsync(
                projectUrn,
                page
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // /// <summary>
    // /// 分頁獲取不在指定專案的客戶
    // /// </summary>
    // /// <param name="projectId">專案ID</param>
    // /// <param name="page">頁碼</param>
    // /// <returns>分頁獲取不在指定專案的客戶</returns>
    // [HttpGet("not-in-project/{projectId}/paged")]
    // public async Task<ActionResult<PagedResult<Customer>>> GetCustomersNotInProject(
    //     int projectId,
    //     [FromQuery] int page = 1
    // )
    // {
    //     try
    //     {
    //         var result = await _customerService.GetPagedCustomersNotInProjectAsync(projectId, page);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// 分頁獲取不在指定專案的客戶
    /// </summary>
    /// <param name="projectUrn">專案URN</param>
    /// <param name="page">頁碼</param>
    /// <returns>分頁獲取不在指定專案的客戶</returns>
    [HttpGet("not-in-project/{projectUrn}/paged")]
    public async Task<ActionResult<PagedResult<Customer>>> GetCustomersNotInProjectUrn(
        string projectUrn,
        [FromQuery] int page = 1
    )
    {
        try
        {
            var result = await _customerService.GetPagedCustomersNotInProjectUrnAsync(
                projectUrn,
                page
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 新增客戶
    /// </summary>
    /// <param name="customer"> 客戶</param>
    /// <returns> 新增的客戶</returns>
    [HttpPost]
    public async Task<IActionResult> AddCustomer([FromBody] Customer customer)
    {
        try
        {
            Customer newCustomer = await _customerService.AddCustomerAsync(customer);
            // 返回201 Created 狀態碼，並在回應實體中包含新建立的客戶資料
            return CreatedAtAction(nameof(GetCustomers), new { id = newCustomer.Id }, newCustomer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 將多個客戶與專案建立關聯
    /// </summary>
    /// <param name="projectUrn">專案URN</param>
    /// <param name="customerIds">客戶ID列表</param>
    /// <returns>操作結果</returns>
    [HttpPost("project/{projectUrn}/associate")]
    public async Task<IActionResult> AssociateCustomersWithProject(
        string projectUrn,
        [FromBody] List<int> customerIds
    )
    {
        try
        {
            var result = await _customerService.AssociateCustomersWithProjectAsync(
                projectUrn,
                customerIds
            );
            return Ok(new { Message = "客戶與專案關聯建立成功", AssociatedCount = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新客戶
    /// </summary>
    /// <param name="id"> 客戶 Id</param>
    /// <param name="customer"> 客戶</param>
    /// <returns> 更新的客戶</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateCustomer(
        [FromQuery] int id,
        [FromBody] Customer customer
    )
    {
        try
        {
            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customer);
            return Ok(updatedCustomer);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 刪除客戶
    /// </summary>
    /// <param name="id"> 客戶 Id</param>
    /// <returns> 刪除的客戶</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteCustomer([FromQuery] int id)
    {
        try
        {
            var deleted = await _customerService.DeleteCustomerAsync(id);
            if (deleted)
            {
                return Ok("Customer deleted successfully.");
            }
            return NotFound("Customer not found.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
