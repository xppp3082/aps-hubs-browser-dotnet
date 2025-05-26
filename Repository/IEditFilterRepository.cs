using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEditFilterRepository
{
    Task<int> GetProjectIdByUrn(string urn);
    Task<int> InsertModel(string urn, int projectId);
    Task DeleteEditFilterAndElements(int modelId);
    Task<int> InsertEditFilter(int modelId, string category, int categoryId, string symbolName);
    Task InsertEditableElements(int editFilterId, List<int> dbIds);
    Task<int> CreateEditFilter(EditFilter filterObject);
    Task<EditFilter> GetEditFilterIdByModelUrn(string modelUrn);
}
