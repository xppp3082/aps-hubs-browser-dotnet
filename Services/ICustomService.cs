using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICustomerService
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<int> AssociateCustomersWithProjectAsync(string projectUrn, List<int> customerIds);
    Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber);
    Task<Customer> UpdateCustomerAsync(int id, Customer customer);
    Task<bool> DeleteCustomerAsync(int id);

    Task<PagedResult<Customer>> GetPagedCustomersByProjectAsync(int projectId, int pageNumber);
    Task<PagedResult<Customer>> GetPagedCustomersNotInProjectAsync(int projectId, int pageNumber);

    Task<PagedResult<Customer>> GetPagedCustomersByProjectUrnAsync(
        string projectUrn,
        int pageNumber
    );
    Task<PagedResult<Customer>> GetPagedCustomersNotInProjectUrnAsync(
        string projectUrn,
        int pageNumber
    );
}
