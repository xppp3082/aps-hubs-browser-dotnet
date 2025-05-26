using System.Threading.Tasks;

public interface IEditFilterService
{
    Task<int> CreateEditFilterAsync(EditFilter filterObject);
    Task<EditFilter> GetEditFilterIdByModelUrnAsync(string modelUrn);
}

