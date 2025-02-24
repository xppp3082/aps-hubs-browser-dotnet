using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICustomerService
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber);
    Task<Customer> UpdateCustomerAsync(int id, Customer customer);
    Task<bool> DeleteCustomerAsync(int id);
}