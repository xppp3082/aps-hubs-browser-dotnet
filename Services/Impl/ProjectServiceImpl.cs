using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProjectServiceImpl : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectServiceImpl(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Project> CheckOrCreateProjectAsync(ProjectInfo projectInfo)
    {
        var existingProject = await _projectRepository.GetProjectByUrnAsync(projectInfo.Urn);
        if (existingProject != null)
        {
            return existingProject;
        }
        var newProject = new Project
        {
            Urn = projectInfo.Urn,
            Name = projectInfo.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        return await _projectRepository.AddProjectAsync(newProject);
    }
}
