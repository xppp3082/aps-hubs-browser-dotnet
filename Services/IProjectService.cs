using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IProjectService
{
    Task<Project> CheckOrCreateProjectAsync(ProjectInfo projectInfo);
}
