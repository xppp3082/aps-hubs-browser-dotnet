using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public class EditFilterRepositoryImpl : IEditFilterRepository
{
    private readonly string _connectionString;

    public EditFilterRepositoryImpl(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> GetProjectIdByUrn(string projectUrn)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new MySqlCommand(
            "SELECT id FROM project WHERE urn = @projectUrn",
            connection);
        command.Parameters.AddWithValue("@projectUrn", projectUrn);
        
        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<int> InsertModel(string modelUrn, int projectId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var selectCmd = new MySqlCommand(
            "SELECT id FROM model WHERE urn = @modelUrn",
            connection);
        selectCmd.Parameters.AddWithValue("@modelUrn", modelUrn);

        var result = await selectCmd.ExecuteScalarAsync();
        if (result != null)
        {
            return Convert.ToInt32(result);
        }

        var insertCmd = new MySqlCommand(
            @"INSERT INTO model (urn, project_id) 
            VALUES (@modelUrn, @projectId);
            SELECT LAST_INSERT_ID();",
            connection);

        insertCmd.Parameters.AddWithValue("@modelUrn", modelUrn);
        insertCmd.Parameters.AddWithValue("@projectId", projectId);

        return Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
    }

    public async Task DeleteEditFilterAndElements(int modelId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var selectCmd = new MySqlCommand(
            @"SELECT id FROM edit_filter WHERE model_id = @modelId",
            connection);
        selectCmd.Parameters.AddWithValue("@modelId", modelId);

        var ids = new List<int>();
        using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
        }

        if (ids.Count == 0) return;

        var deleteElementCmd = new MySqlCommand(
            $"DELETE FROM editable_element WHERE edit_filter_id IN ({string.Join(",", ids)})",
            connection);
        await deleteElementCmd.ExecuteNonQueryAsync();

        var deleteFilterCmd = new MySqlCommand(
            $"DELETE FROM edit_filter WHERE id IN ({string.Join(",", ids)})",
            connection);
        await deleteFilterCmd.ExecuteNonQueryAsync();   
    }

    public async Task<int> InsertEditFilter(int modelId, string category, int categoryId, string symbolName)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new MySqlCommand(
            @"INSERT INTO edit_filter (model_id, category, categoryId, symbol_name) 
              VALUES (@modelId, @category, @categoryId, @symbolName);
              SELECT LAST_INSERT_ID();",
            connection);

        command.Parameters.AddWithValue("@modelId", modelId);
        command.Parameters.AddWithValue("@category", category);
        command.Parameters.AddWithValue("@categoryId", categoryId);
        command.Parameters.AddWithValue("@symbolName", symbolName);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task InsertEditableElements(int editFilterId, List<int> dbIds)
    {
        if (dbIds == null || dbIds.Count == 0) return;

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var values = string.Join(", ", dbIds.Select((dbid, idx) => $"(@editFilterId, @dbid{idx})"));
        var command = new MySqlCommand(
            $"INSERT INTO editable_element (edit_filter_id, dbid) VALUES {values}",
            connection);

        command.Parameters.AddWithValue("@editFilterId", editFilterId);
        for (int i = 0; i < dbIds.Count; i++)
        {
            command.Parameters.AddWithValue($"@dbid{i}", dbIds[i]);
        }

        await command.ExecuteNonQueryAsync();
    }

    // 集合前述所有判斷，進行 transaction，創建 EditFilter
    public async Task<int> CreateEditFilter(FilterObject filterObject)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var projectId = await GetProjectIdByUrn(filterObject.ProjectUrn);
            if (projectId == 0)
            {
                throw new Exception("Project not found");
            }
            var modelId = await InsertModel(filterObject.ModelUrn, projectId);
            // 刪除舊的 edit_filter 與 editable_element (每個模型只能設訂一次 editFilter)
            await DeleteEditFilterAndElements(modelId);

            // 3. 針對每個 checkedObject 插入 edit_filter 與 editable_element
            foreach (var obj in filterObject.CheckedObjects)
            {
                // 插入新的 edit_filter 與 editable_element
                var editFilterId = await InsertEditFilter(modelId, obj.Category, obj.CategoryId, obj.SymbolName);
                await InsertEditableElements(editFilterId, obj.DbIds);
            }
            await transaction.CommitAsync();
            return 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}


