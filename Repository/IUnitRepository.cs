using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IUnitRepository
{
    Task<List<Unit>> GetAllUnitsAsync();
    Task<List<Unit>> GetUnitsByCustomerProjectIdAsync(int projectId, int customerId);

    // Task<PagedResult<Unit>> GetPagedUnitsAsync(int pageNumber, int pageSize);

    Task<Unit> AddUnitAsync(Unit unit, int customerId, int projectId);
    Task<Unit> UpdateUnitAsync(Unit unit, int projectId, int customerId);
    Task<bool> DeleteUnitAsync(int id);
    Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(
        int projectId,
        int customerId,
        int pageNumber,
        int pageSize
    );
}
