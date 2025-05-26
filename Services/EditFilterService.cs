using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace aps_hubs_browser_dotnet.Services
{
    public class EditFilterService : IEditFilterService
    {
        private readonly IEditFilterRepository _editFilterRepository;
        private readonly ILogger<EditFilterService> _logger;

        public EditFilterService(
            IEditFilterRepository editFilterRepository,
            ILogger<EditFilterService> logger)
        {
            _editFilterRepository = editFilterRepository;
            _logger = logger;
        }

        public async Task<int> CreateEditFilterAsync(FilterObject filterObject)
        {
            _logger.LogInformation("開始處理 EditFilter 創建請求");
            try
            {
                var result = await _editFilterRepository.CreateEditFilter(filterObject);
                _logger.LogInformation("EditFilter 創建完成");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditFilter 創建失敗: {Message}", ex.Message);
                throw;
            }
        }
    }
} 