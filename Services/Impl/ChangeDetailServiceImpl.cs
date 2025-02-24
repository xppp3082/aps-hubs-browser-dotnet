using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ChangeDetailServiceImpl : IChangeDetailService
{
    private readonly IConfiguration _configuration;
    private readonly IChangeDetailRepository _changeDetailRepository;
    private readonly int _defaultPageSize;

    public ChangeDetailServiceImpl(
        IChangeDetailRepository changeDetailRepository,
        IConfiguration configuration
    )
    {
        _changeDetailRepository = changeDetailRepository;
        _configuration = configuration;
        _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
    }

    public async Task<List<ChangeDetail>> GetAllChangeDetailsAsync()
    {
        return await _changeDetailRepository.GetAllChangeDetailsAsync();
    }

    public async Task<ChangeDetail> GetChangeDetailByIdAsync(int id)
    {
        return await _changeDetailRepository.GetChangeDetailByIdAsync(id);
    }

    public async Task<ChangeDetail> CreateChangeDetailAsync(ChangeDetail changeDetail)
    {
        return await _changeDetailRepository.CreateChangeDetailAsync(changeDetail);
    }

    public async Task<ChangeDetail> UpdateChangeDetailAsync(ChangeDetail changeDetail)
    {
        return await _changeDetailRepository.UpdateChangeDetailAsync(changeDetail);
    }

    public async Task<bool> DeleteChangeDetailAsync(int id)
    {
        return await _changeDetailRepository.DeleteChangeDetailAsync(id);
    }

    public async Task<List<ChangeDetail>> GetChangeDetailsByUnitIdAsync(int unitId)
    {
        return await _changeDetailRepository.GetChangeDetailsByUnitIdAsync(unitId);
    }

    public async Task<PagedResult<ChangeDetail>> GetPagedChangeDetailsByUnitIdAsync(
        int unitId,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _changeDetailRepository.GetPagedChangeDetailsByUnitIdAsync(
            unitId,
            pageNumber,
            _defaultPageSize
        );
    }
}
