using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public interface IUnitService
{
    Task<List<Unit>> GetAllUnitsAsync();
    Task<List<Unit>> GetUnitsByProjectIdAsync(int projectId);
    Task<Unit> AddUnitAsync(Unit unit);
    Task<bool> DeleteUnitAsync(int id);
    Task<Unit> UpdateUnitAsync(Unit unit);
    Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(int projectId, int customerId, int pageNumber);
}
