using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public interface IUnitRepository
{
    Task<List<Unit>> GetAllUnitsAsync();
    Task<List<Unit>> GetUnitsByProjectIdAsync(int projectId);
    // Task<PagedResult<Unit>> GetPagedUnitsAsync(int pageNumber, int pageSize);
    Task<Unit> AddUnitAsync(Unit unit);
    Task<Unit> UpdateUnitAsync(Unit unit);
    Task<bool> DeleteUnitAsync(int id);
    Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(int projectId, int customerId, int pageNumber, int pageSize);
}
