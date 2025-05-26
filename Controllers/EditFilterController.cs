using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using Serilog;

namespace aps_hubs_browser_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditFilterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IEditFilterService _editFilterService;
        private readonly ILogger<EditFilterController> _logger;

        public EditFilterController(
            IConfiguration configuration,
            IEditFilterService editFilterService,
            ILogger<EditFilterController> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _editFilterService = editFilterService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEditFilter([FromBody] EditFilter filterObject)
        {
            Log.Information("Log from serilog: 開始創建 EditFilter: {@FilterObject}", filterObject);
            _logger.LogInformation("開始創建 EditFilter: {@FilterObject}", filterObject);
            try
            {
                await _editFilterService.CreateEditFilterAsync(filterObject);
                return Ok(new { message = "Edit filter created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建 EditFilter 時發生錯誤: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
            [HttpGet]
            public async Task<IActionResult> GetEditFilterIdByModelUrn([FromQuery] string modelUrn)
            {
                var editFilter = await _editFilterService.GetEditFilterIdByModelUrnAsync(modelUrn);
                return Ok(editFilter);
            }   
    }

} 