using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IChangeDetailService
{
    Task<List<ChangeDetail>> GetAllChangeDetailsAsync();
    Task<ChangeDetail> GetChangeDetailByIdAsync(int id);
    Task<List<ChangeDetail>> GetChangeDetailsByUnitIdAsync(int unitId);
    Task<PagedResult<ChangeDetail>> GetPagedChangeDetailsByUnitIdAsync(int unitId, int pageNumber);
    Task<ChangeDetail> CreateChangeDetailAsync(ChangeDetail changeDetail);
    Task<ChangeDetail> UpdateChangeDetailAsync(ChangeDetail changeDetail);
    Task<bool> DeleteChangeDetailAsync(int id);
}
