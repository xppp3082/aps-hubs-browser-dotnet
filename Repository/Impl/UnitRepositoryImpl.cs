using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
// using MySql.Data.MySqlClient;
using MySqlConnector;

public class UnitRepositoryImpl : IUnitRepository
{
    private readonly string _connectionString;

    public UnitRepositoryImpl(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query = "SELECT * FROM unit";
            var units = await connection.QueryAsync<Unit>(query);
            return units.ToList();
        }
    }

    public async Task<List<Unit>> GetUnitsByCustomerProjectIdAsync(int customerId, int projectId)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query =
                    @"SELECT * FROM unit WHERE customer_project_id IN 
                    (SELECT id FROM customer_project cp WHERE cp.customer_id = @CustomerId AND cp.project_id = @ProjectId)";
                var units = await connection.QueryAsync<Unit>(
                    query,
                    new { ProjectId = projectId, CustomerId = customerId }
                );
                return units.ToList();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<Unit> AddUnitAsync(Unit unit, int customerId, int projectId)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                unit.CreatedAt = DateTime.UtcNow;
                unit.UpdatedAt = DateTime.UtcNow;
                string query =
                    @"
            INSERT INTO unit (unit_number, floor, customer_project_id) 
            VALUES (@UnitNumber, @Floor, (SELECT id FROM customer_project cp WHERE cp.customer_id = @CustomerId AND cp.project_id = @ProjectId));
            SELECT * FROM unit WHERE id = LAST_INSERT_ID();
            ";
                // var result = await connection.ExecuteAsync(query, unit);
                Unit result = await connection.QuerySingleAsync<Unit>(
                    query,
                    new
                    {
                        UnitNumber = unit.UnitNumber,
                        Floor = unit.Floor,
                        CustomerId = customerId,
                        ProjectId = projectId,
                    }
                );
                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    // public async Task<Unit> UpdateUnitAsync(Unit unit)
    // {
    //     using (var connection = new SqlConnection(_connectionString))
    //     {
    //         await connection.OpenAsync();
    //     }
    // }
    public async Task<bool> DeleteUnitAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query = "DELETE FROM unit WHERE id = @Id";
            var result = await connection.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }
    }

    public async Task<Unit> UpdateUnitAsync(Unit unit, int projectId, int customerId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            unit.UpdatedAt = DateTime.UtcNow;
            string query =
                @"
            UPDATE unit SET 
            unit_number = @UnitNumber,
            floor = @Floor,
            customer_project_id = (SELECT id FROM customer_project cp WHERE cp.customer_id = @CustomerId AND cp.project_id = @ProjectId)
            WHERE id = @Id;
            SELECT * FROM unit WHERE id = @Id;
            ";
            var result = await connection.QuerySingleAsync<Unit>(
                query,
                new
                {
                    UnitNumber = unit.UnitNumber, // 明確指定參數
                    Floor = unit.Floor, // 明確指定參數
                    Id = unit.Id,
                    CustomerId = customerId,
                    ProjectId = projectId,
                }
            );
            return result;
        }
    }

    public async Task<PagedResult<Unit>> GetPagedUnitsByProjectIdAndCustomerIdAsync(
        int projectId,
        int customerId,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string countQuery =
                @"
            SELECT COUNT(*) FROM unit 
            WHERE customer_project_id = (SELECT id FROM customer_project cp WHERE cp.customer_id = @CustomerId AND cp.project_id = @ProjectId);
            ";
            var totalItems = await connection.ExecuteScalarAsync<int>(
                countQuery,
                new { ProjectId = projectId, CustomerId = customerId }
            );

            var offset = (pageNumber - 1) * pageSize;

            string query =
                @"
            SELECT * FROM unit 
            WHERE customer_project_id = (SELECT id FROM customer_project cp WHERE cp.customer_id = @CustomerId AND cp.project_id = @ProjectId)
            LIMIT @PageSize OFFSET @Offset;
            ";
            var units = await connection.QueryAsync<Unit>(
                query,
                new
                {
                    ProjectId = projectId,
                    CustomerId = customerId,
                    PageSize = pageSize,
                    Offset = offset,
                }
            );
            return new PagedResult<Unit>
            {
                Items = units.ToList(),
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            };
        }
    }
}
