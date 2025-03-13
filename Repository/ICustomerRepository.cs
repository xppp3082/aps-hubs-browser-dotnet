using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<int> AssociateCustomersWithProjectAsync(string projectUrn, List<int> customerIds);
    Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber, int pageSize);
    Task<Customer> GetCustomerByIdAsync(int id);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int id);

    // 在現有的 ICustomerRepository 介面中新增以下方法
    Task<PagedResult<Customer>> GetPagedCustomersByProjectAsync(
        int projectId,
        int pageNumber,
        int pageSize
    );
    Task<PagedResult<Customer>> GetPagedCustomersNotInProjectAsync(
        int projectId,
        int pageNumber,
        int pageSize
    );

    Task<PagedResult<Customer>> GetPagedCustomersByProjectUrnAsync(
        string projectUrn,
        int pageNumber,
        int pageSize
    );
    Task<PagedResult<Customer>> GetPagedCustomersNotInProjectUrnAsync(
        string projectUrn,
        int pageNumber,
        int pageSize
    );
}
