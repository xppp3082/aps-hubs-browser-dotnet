using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IUnitService
{
    Task<List<Unit>> GetAllUnitsAsync();

    // Task<List<Unit>> GetUnitsByProjectIdAsync(int projectId)
    Task<List<Unit>> GetUnitsByCustomerProjectIdAsync(int customerId, int projectId);
    Task<Unit> AddUnitAsync(Unit unit, int customerId, int projectId);
    Task<bool> DeleteUnitAsync(int id);
    Task<Unit> UpdateUnitAsync(Unit unit, int projectId, int customerId);
    Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(
        int projectId,
        int customerId,
        int pageNumber
    );
}
