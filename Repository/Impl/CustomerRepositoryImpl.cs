using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public class CustomerRepositoryImpl : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepositoryImpl(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query =
                @"INSERT INTO customer (name, phone, email, created_at, updated_at) 
            VALUES (@name, @phone, @email, @created_at, @updated_at);
            SELECT * FROM customer WHERE id = LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                var now = DateTime.UtcNow;
                command.Parameters.AddWithValue("@name", customer.Name);
                command.Parameters.AddWithValue("@phone", customer.Phone);
                command.Parameters.AddWithValue("@email", customer.Email);
                command.Parameters.AddWithValue("@created_at", now);
                command.Parameters.AddWithValue("@updated_at", now);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Phone = reader.GetString("phone"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };
                    }
                }
            }
        }
        throw new Exception("Failed to add customer.");
    }

    public async Task<int> AssociateCustomersWithProjectAsync(
        string projectUrn,
        List<int> customerIds
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // 首先檢查專案是否存在
            string checkProjectQuery = "SELECT COUNT(*) FROM project WHERE urn = @projectUrn";
            using (var checkCommand = new MySqlCommand(checkProjectQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@projectUrn", projectUrn);
                var projectCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (projectCount == 0)
                {
                    throw new KeyNotFoundException($"專案URN {projectUrn} 不存在");
                }
            }

            // 使用事務確保操作的原子性
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    int associatedCount = 0;

                    foreach (var customerId in customerIds)
                    {
                        // 檢查關聯是否已存在
                        string checkQuery =
                            @"
                        SELECT COUNT(*) FROM customer_project 
                        WHERE customer_id = @customerId AND project_id = (SELECT id FROM project WHERE urn = @projectUrn)";

                        using (
                            var checkCommand = new MySqlCommand(
                                checkQuery,
                                connection,
                                transaction as MySqlTransaction
                            )
                        )
                        {
                            checkCommand.Parameters.AddWithValue("@customerId", customerId);
                            checkCommand.Parameters.AddWithValue("@projectUrn", projectUrn);
                            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                            // 如果關聯不存在，則建立關聯
                            if (count == 0)
                            {
                                string insertQuery =
                                    @"
                                INSERT INTO customer_project (customer_id, project_id) 
                                VALUES (@customerId, (SELECT id FROM project WHERE urn = @projectUrn))";

                                using (
                                    var insertCommand = new MySqlCommand(
                                        insertQuery,
                                        connection,
                                        transaction as MySqlTransaction
                                    )
                                )
                                {
                                    insertCommand.Parameters.AddWithValue(
                                        "@customerId",
                                        customerId
                                    );
                                    insertCommand.Parameters.AddWithValue(
                                        "@projectUrn",
                                        projectUrn
                                    );
                                    await insertCommand.ExecuteNonQueryAsync();
                                    associatedCount++;
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return associatedCount;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }

    public async Task<bool> RemoveCustomerFromProjectAsync(string projectUrn, List<int> customerIds)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    string sql =
                        @"DELETE FROM customer_project WHERE project_id = (SELECT id FROM project WHERE urn = @projectUrn) AND customer_id IN @customerIds";
                    var parameters = new { ProjectUrn = projectUrn, CustomerIds = customerIds };
                    // 執行刪除操作
                    var affectedRows = await connection.ExecuteAsync(sql, parameters, transaction);
                    await transaction.CommitAsync();
                    return affectedRows > 0;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"移除客戶與專案關聯時發生錯誤: {ex.Message}");
                }
            }
        }
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        List<Customer> customers = new List<Customer>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "SELECT * FROM customer";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    customers.Add(
                        new Customer
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Phone = reader.GetString("phone"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        }
                    );
                }
            }
        }
        return customers;
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersAsync(int pageNumber, int pageSize)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // 獲取總紀錄數
            string countQuery = "SELECT COUNT(*) FROM customer";
            var totalItems = Convert.ToInt32(
                await new MySqlCommand(countQuery, connection).ExecuteScalarAsync()
            );

            // 計算分頁
            var offset = (pageNumber - 1) * pageSize;
            string query =
                @"SELECT * FROM customer ORDER BY updated_at DESC, id DESC LIMIT @offset, @pageSize";

            var customers = new List<Customer>();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@offset", offset);
                command.Parameters.AddWithValue("@pageSize", pageSize);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(
                            new Customer
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Phone = reader.GetString("phone"),
                                Email = reader.GetString("email"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                UpdatedAt = reader.GetDateTime("updated_at"),
                            }
                        );
                    }
                }
            }
            return new PagedResult<Customer>
            {
                Items = customers,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            };
        }
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersByProjectAsync(
        int projectId,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string countQuery =
                @"SELECT COUNT(DISTINCT c.id) 
                FROM customer c
                JOIN customer_project cp ON c.id = cp.customer_id
                WHERE cp.project_id = @projectId";
            using (var countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddWithValue("@projectId", projectId);
                var totalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                // 計算分頁
                var offset = (pageNumber - 1) * pageSize;
                string query =
                    @"
                SELECT DISTINCT c.* 
                FROM customer c
                JOIN customer_project cp ON c.id = cp.customer_id
                WHERE cp.project_id = @projectId
                ORDER BY cp.updated_at DESC, c.id DESC
                LIMIT @offset, @pageSize";

                var customers = new List<Customer>();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.Parameters.AddWithValue("@offset", offset);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(
                                new Customer
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                }
                            );
                        }
                    }
                }
                return new PagedResult<Customer>
                {
                    Items = customers,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                };
            }
        }
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersNotInProjectAsync(
        int projectId,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // 獲取總紀錄數
            string countQuery =
                @"
            SELECT COUNT(*) 
            FROM customer c
            WHERE NOT EXISTS (
                SELECT 1 FROM customer_project cp 
                WHERE cp.customer_id = c.id AND cp.project_id = @projectId
            )";

            using (var countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddWithValue("@projectId", projectId);
                var totalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                // 計算分頁
                var offset = (pageNumber - 1) * pageSize;
                string query =
                    @"
                SELECT c.* 
                FROM customer c
                WHERE NOT EXISTS (
                    SELECT 1 FROM customer_project cp 
                    WHERE cp.customer_id = c.id AND cp.project_id = @projectId
                )
                ORDER BY c.id 
                LIMIT @offset, @pageSize";

                var customers = new List<Customer>();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.Parameters.AddWithValue("@offset", offset);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(
                                new Customer
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                }
                            );
                        }
                    }
                }

                return new PagedResult<Customer>
                {
                    Items = customers,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                };
            }
        }
    }

    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "SELECT * FROM customer WHERE id = @id";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Phone = reader.GetString("phone"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };
                    }
                    return null;
                }
            }
        }
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query =
                @"UPDATE customer
            SET name = @name, phone = @phone, email = @email, updated_at = @updated_at
            WHERE id = @id;
            SELECT * FROM customer WHERE id = @id;";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", customer.Name);
                command.Parameters.AddWithValue("@phone", customer.Phone);
                command.Parameters.AddWithValue("@email", customer.Email);
                command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                command.Parameters.AddWithValue("@id", customer.Id);
                // await command.ExecuteNonQueryAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Phone = reader.GetString("phone"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };
                    }
                }
            }
        }
        throw new Exception("Failed to update customer.");
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "DELETE FROM customer WHERE id = @id";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }
        throw new Exception("Failed to delete customer.");
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersByProjectUrnAsync(
        string projectUrn,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string countQuery =
                @"SELECT COUNT(DISTINCT c.id) 
                FROM customer c
                JOIN customer_project cp ON c.id = cp.customer_id
                WHERE cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)";
            using (var countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddWithValue("@projectUrn", projectUrn);
                var totalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                // 計算分頁
                var offset = (pageNumber - 1) * pageSize;
                string query =
                    @"
                SELECT DISTINCT c.* ,cp.updated_at
                FROM customer c
                JOIN customer_project cp ON c.id = cp.customer_id
                WHERE cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)
                ORDER BY cp.updated_at DESC
                LIMIT @offset, @pageSize";

                var customers = new List<Customer>();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@projectUrn", projectUrn);
                    command.Parameters.AddWithValue("@offset", offset);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(
                                new Customer
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                }
                            );
                        }
                    }
                }
                return new PagedResult<Customer>
                {
                    Items = customers,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                };
            }
        }
    }

    public async Task<List<Customer>> GetAllCustomersNotInProjectUrnAsync(string projectUrn)
    {
        List<Customer> customerList = new List<Customer>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query =
                @"
            SELECT * FROM customer c
            WHERE NOT EXISTS (
                SELECT 1 FROM customer_project cp 
                WHERE cp.customer_id = c.id AND cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)
            )";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@projectUrn", projectUrn);
                var customers = await command.ExecuteReaderAsync();

                while (await customers.ReadAsync())
                {
                    customerList.Add(
                        new Customer
                        {
                            Id = customers.GetInt32("id"),
                            Name = customers.GetString("name"),
                            Phone = customers.GetString("phone"),
                            Email = customers.GetString("email"),
                            CreatedAt = customers.GetDateTime("created_at"),
                            UpdatedAt = customers.GetDateTime("updated_at"),
                        }
                    );
                }
            }
        }
        return customerList;
    }

    public async Task<PagedResult<Customer>> GetPagedCustomersNotInProjectUrnAsync(
        string projectUrn,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // 獲取總紀錄數
            string countQuery =
                @"
            SELECT COUNT(*) 
            FROM customer c
            WHERE NOT EXISTS (
                SELECT 1 FROM customer_project cp 
                WHERE cp.customer_id = c.id AND cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)
            )";

            using (var countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddWithValue("@projectUrn", projectUrn);
                var totalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                // 計算分頁
                var offset = (pageNumber - 1) * pageSize;
                string query =
                    @"
                SELECT c.* 
                FROM customer c
                WHERE NOT EXISTS (
                    SELECT 1 FROM customer_project cp 
                    WHERE cp.customer_id = c.id AND cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)
                )
                ORDER BY c.id 
                LIMIT @offset, @pageSize";

                var customers = new List<Customer>();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@projectUrn", projectUrn);
                    command.Parameters.AddWithValue("@offset", offset);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(
                                new Customer
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                }
                            );
                        }
                    }
                }

                return new PagedResult<Customer>
                {
                    Items = customers,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                };
            }
        }
    }

    public async Task<List<Customer>> SearchCustomersNotInProjectUrnAsync(string projectUrn, string searchTerm)
    {
        List<Customer> customerList = new List<Customer>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query =
                @"
            SELECT * FROM customer c
            WHERE NOT EXISTS (
                SELECT 1 FROM customer_project cp 
                WHERE cp.customer_id = c.id AND cp.project_id = (SELECT id FROM project WHERE urn = @projectUrn)
            )
            AND (
                c.name LIKE @searchTerm 
                OR c.email LIKE @searchTerm
            )";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@projectUrn", projectUrn);
                command.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                var customers = await command.ExecuteReaderAsync();

                while (await customers.ReadAsync())
                {
                    customerList.Add(
                        new Customer
                        {
                            Id = customers.GetInt32("id"),
                            Name = customers.GetString("name"),
                            Phone = customers.GetString("phone"),
                            Email = customers.GetString("email"),
                            CreatedAt = customers.GetDateTime("created_at"),
                            UpdatedAt = customers.GetDateTime("updated_at"),
                        }
                    );
                }
            }
        }
        return customerList;
    }
}
