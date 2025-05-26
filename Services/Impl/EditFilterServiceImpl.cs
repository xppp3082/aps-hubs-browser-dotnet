using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

public class EditFilterServiceImpl : IEditFilterService
{
    private readonly IEditFilterRepository _editFilterRepository;

    public EditFilterServiceImpl(IEditFilterRepository editFilterRepository) {
        _editFilterRepository = editFilterRepository;
    }

    public async Task<int> CreateEditFilterAsync(EditFilter filterObject)
    {
        return await _editFilterRepository.CreateEditFilter(filterObject);;
    }

    public async Task<EditFilter> GetEditFilterIdByModelUrnAsync(string modelUrn)
    {
        return await _editFilterRepository.GetEditFilterIdByModelUrn(modelUrn);
    }
}

