using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class UnitServiceImpl : IUnitService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitRepository _unitRepository;
    private readonly int _defaultPageSize;

    public UnitServiceImpl(IUnitRepository unitRepository, IConfiguration configuration)
    {
        _unitRepository = unitRepository;
        _configuration = configuration;
        _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
    }

    public async Task<Unit> AddUnitAsync(Unit unit, int customerId, int projectId)
    {
        return await _unitRepository.AddUnitAsync(unit, customerId, projectId);
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        return await _unitRepository.DeleteUnitAsync(id);
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        return await _unitRepository.GetAllUnitsAsync();
    }

    public async Task<List<Unit>> GetUnitsByCustomerProjectIdAsync(int customerId, int projectId)
    {
        return await _unitRepository.GetUnitsByCustomerProjectIdAsync(customerId, projectId);
    }

    public async Task<Unit> UpdateUnitAsync(Unit unit, int projectId, int customerId)
    {
        return await _unitRepository.UpdateUnitAsync(unit, projectId, customerId);
    }

    public async Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(
        int projectId,
        int customerId,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _unitRepository.GetPagedUnitsByProjectIdAndCustomerIdAsync(
            projectId,
            customerId,
            pageNumber,
            _defaultPageSize
        );
    }
}
