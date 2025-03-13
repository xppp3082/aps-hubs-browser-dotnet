using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IProjectRepository
{
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project> AddProjectAsync(Project project);
    Task<Project> GetProjectByUrnAsync(string urn);
    //Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize);
    //Task<Project> GetProjectByIdAsync(int id);
    //Task<Project> UpdateProjectAsync(Project project);
    //Task<bool> DeleteProjectAsync(int id);
}
