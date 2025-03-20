using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class CustomerServiceImpl : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;
    private readonly int _defaultPageSize;

    public CustomerServiceImpl(ICustomerRepository customerRepository, IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
        _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        // 在這邊添加業務邏輯，例如:
        // 驗證客戶資料
        if (
            string.IsNullOrEmpty(customer.Name)
            || string.IsNullOrEmpty(customer.Phone)
            || string.IsNullOrEmpty(customer.Email)
        )
        {
            throw new ArgumentException("All fields are required.");
        }

        return await _customerRepository.AddCustomerAsync(customer);
    }

    public async Task<int> AssociateCustomersWithProjectAsync(
        string projectUrn,
        List<int> customerIds
    )
    {
        // 驗證專案是否存在

        // 或者直接在 CustomerRepository 的 AssociateCustomersWithProjectAsync 方法中處理

        if (customerIds == null || customerIds.Count == 0)
        {
            throw new ArgumentException("客戶ID列表不能為空");
        }

        // 建立關聯
        return await _customerRepository.AssociateCustomersWithProjectAsync(
            projectUrn,
            customerIds
        );
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found");
        }
        return await _customerRepository.DeleteCustomerAsync(id);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        // 可以在這邊加入商業邏輯過濾
        return await _customerRepository.GetAllCustomersAsync();
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber)
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _customerRepository.GetPagedCustomersAsync(pageNumber, _defaultPageSize);
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersByProjectAsync(
        int projectId,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _customerRepository.GetPagedCustomersByProjectAsync(
            projectId,
            pageNumber,
            _defaultPageSize
        );
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersByProjectUrnAsync(
        string projectUrn,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _customerRepository.GetPagedCustomersByProjectUrnAsync(
            projectUrn,
            pageNumber,
            _defaultPageSize
        );
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersNotInProjectAsync(
        int projectId,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _customerRepository.GetPagedCustomersNotInProjectAsync(
            projectId,
            pageNumber,
            _defaultPageSize
        );
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersNotInProjectUrnAsync(
        string projectUrn,
        int pageNumber
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        return await _customerRepository.GetPagedCustomersNotInProjectUrnAsync(
            projectUrn,
            pageNumber,
            _defaultPageSize
        );
    }

    public async Task<Customer> UpdateCustomerAsync(int id, Customer customer)
    {
        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found");
        }

        // 只更新非空值
        if (!string.IsNullOrEmpty(customer.Name))
            existingCustomer.Name = customer.Name;
        if (!string.IsNullOrEmpty(customer.Phone))
            existingCustomer.Phone = customer.Phone;
        if (!string.IsNullOrEmpty(customer.Email))
            existingCustomer.Email = customer.Email;

        return await _customerRepository.UpdateCustomerAsync(existingCustomer);
    }

    public async Task<bool> RemoveCustomerFromProjectAsync(string projectUrn, List<int> customerIds)
    {
        return await _customerRepository.RemoveCustomerFromProjectAsync(projectUrn, customerIds);
    }
}
