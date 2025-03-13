using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public class ProjectRepositoryImpl : IProjectRepository
{
    private readonly string _connectionString;

    public ProjectRepositoryImpl(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        List<Project> projects = new List<Project>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "SELECT * FROM customer";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    projects.Add(
                        new Project
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        }
                    );
                }
            }
        }
        return projects;
    }

    public async Task<Project> AddProjectAsync(Project project)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if project with the same urn already exists
            string checkQuery = "SELECT COUNT(*) FROM project WHERE urn = @urn";
            using (var checkCommand = new MySqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@urn", project.Urn);
                int existingCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (existingCount > 0)
                {
                    throw new Exception("A project with the same urn already exists.");
                }
            }

            string query =
                @"INSERT INTO project (urn, name, created_at, updated_at)
            VALUES (@urn, @name, @created_at, @updated_at);
            SELECT * FROM project WHERE id = LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                var now = DateTime.UtcNow;
                command.Parameters.AddWithValue("@urn", project.Urn);
                command.Parameters.AddWithValue("@name", project.Name);
                command.Parameters.AddWithValue("@created_at", now);
                command.Parameters.AddWithValue("@updated_at", now);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Project
                        {
                            Id = reader.GetInt32("id"),
                            Urn = reader.GetString("urn"),
                            Name = reader.GetString("name"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };
                    }
                }
            }
        }
        throw new Exception("Failed to add project.");
    }

    public async Task<Project> GetProjectByUrnAsync(string urn)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT * FROM project WHERE urn = @urn";
                var project = await connection.QueryFirstOrDefaultAsync<Project>(
                    query,
                    new { Urn = urn }
                );
                return project;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get project by urn.", ex);
        }
    }

    //public async Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize)
    //{
    //    using (var connection = new MySqlConnection(_connectionString))
    //    {
    //        await connection.OpenAsync();
    //    }
    //}

    //public async Task<Project> GetProjectByIdAsync(int id)
    //{
    //    using (var connection = new MySqlConnection(_connectionString))
    //    {
    //        await connection.OpenAsync();
    //    }
    //}

    //public async Task<Project> UpdateProjectAsync(Project project)
    //{
    //    using (var connection = new MySqlConnection(_connectionString))
    //    {
    //        await connection.OpenAsync();
    //    }
    //}

    //public async Task<bool> DeleteProjectAsync(int id)
    //{
    //    using (var connection = new MySqlConnection(_connectionString))
    //    {
    //        await connection.OpenAsync();
    //    }
    //}
}
