using System.Threading.Tasks;

public interface IEditFilterService
{
    Task<int> CreateEditFilterAsync(FilterObject filterObject);
}

