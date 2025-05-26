using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
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

        public async Task<int> CreateEditFilterAsync(EditFilter filterObject)
        {
            try
            {
                var result = await _editFilterRepository.CreateEditFilter(filterObject);
                Log.Information("EditFilter 創建完成");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EditFilter 創建失敗: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EditFilter> GetEditFilterIdByModelUrnAsync(string modelUrn)
        {
            return await _editFilterRepository.GetEditFilterIdByModelUrn(modelUrn);
        }
    }
} 