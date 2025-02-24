using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Misc;

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
                            UpdatedAt = reader.GetDateTime("updated_at")
                        };
                    }
                }
            }
        }
        throw new Exception("Failed to add customer.");
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
                    customers.Add(new Customer
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Phone = reader.GetString("phone"),
                        Email = reader.GetString("email"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        UpdatedAt = reader.GetDateTime("updated_at")
                    });
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
            var totalItems = Convert.ToInt32(await new MySqlCommand(countQuery, connection).ExecuteScalarAsync());

            // 計算分頁
            var offset = (pageNumber - 1) * pageSize;
            string query = @"SELECT * FROM customer ORDER BY id LIMIT @offset, @pageSize";

            var customers = new List<Customer>();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@offset", offset);
                command.Parameters.AddWithValue("@pageSize", pageSize);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Phone = reader.GetString("phone"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at")
                        });
                    }
                }
            }
            return new PagedResult<Customer>
            {
                Items = customers,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
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
                            UpdatedAt = reader.GetDateTime("updated_at")
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
                            UpdatedAt = reader.GetDateTime("updated_at")
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
}
