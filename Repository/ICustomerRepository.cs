using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber, int pageSize);
    Task<Customer> GetCustomerByIdAsync(int id);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int id);

}
