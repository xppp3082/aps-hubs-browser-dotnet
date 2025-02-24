using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public class ChangeDetailRepositoryImpl : IChangeDetailRepository
{
    private readonly string _connectionString;

    public ChangeDetailRepositoryImpl(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<ChangeDetail>> GetAllChangeDetailsAsync()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "SELECT * FROM change_detail";
            var changeDetails = await connection.QueryAsync<ChangeDetail>(query);
            return changeDetails.ToList();
        }
    }

    public async Task<ChangeDetail> GetChangeDetailByIdAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM change_detail WHERE id = @id";
            var changeDetail = await connection.QueryFirstOrDefaultAsync<ChangeDetail>(
                query,
                new { id }
            );
            return changeDetail;
        }
    }

    public async Task<List<ChangeDetail>> GetChangeDetailsByUnitIdAsync(int unitId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query = "SELECT * FROM change_detail WHERE unit_id = @unitId";
            var changeDetails = await connection.QueryAsync<ChangeDetail>(query, new { unitId });
            return changeDetails.ToList();
        }
    }

    public async Task<ChangeDetail> CreateChangeDetailAsync(ChangeDetail changeDetail)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query =
                @"INSERT INTO change_detail (db_id, element_id, status, vector, unit_id, unit) 
                VALUES (@DbId, @ElementId, @Status, @Vector, @UnitId, @Unit);
                SELECT * FROM change_detail WHERE id = LAST_INSERT_ID();";
            await connection.ExecuteAsync(query, changeDetail);
            return changeDetail;
        }
    }

    public async Task<ChangeDetail> UpdateChangeDetailAsync(ChangeDetail changeDetail)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query =
                @"UPDATE change_detail 
                SET db_id = @DbId, element_id = @ElementId, status = @Status, vector = @Vector, unit_id = @UnitId, unit = @Unit 
                WHERE id = @Id";
            await connection.ExecuteAsync(query, changeDetail);
            return changeDetail;
        }
    }

    public async Task<bool> DeleteChangeDetailAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string query = "DELETE FROM change_detail WHERE id = @id";
            await connection.ExecuteAsync(query, new { id });
            return true;
        }
    }

    public async Task<PagedResult<ChangeDetail>> GetPagedChangeDetailsByUnitIdAsync(
        int unitId,
        int pageNumber,
        int pageSize
    )
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string countQuery = "SELECT COUNT(*) FROM change_detail WHERE unit_id = @unitId";
            var totalItems = await connection.ExecuteScalarAsync<int>(countQuery, new { unitId });

            var offset = (pageNumber - 1) * pageSize;

            string query =
                "SELECT * FROM change_detail WHERE unit_id = @unitId LIMIT @pageSize OFFSET @offset";
            var changeDetails = await connection.QueryAsync<ChangeDetail>(
                query,
                new
                {
                    unitId,
                    pageSize,
                    offset,
                }
            );
            return new PagedResult<ChangeDetail>
            {
                Items = changeDetails.ToList(),
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            };
        }
    }
}
