using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.FluentMap.Mapping;

public class ProjectMap : EntityMap<Project>
{
    public ProjectMap()
    {
        Map(p => p.Id).ToColumn("id");
        Map(p => p.Urn).ToColumn("urn");
        Map(p => p.Name).ToColumn("name");
        Map(p => p.CreatedAt).ToColumn("created_at");
        Map(p => p.UpdatedAt).ToColumn("updated_at");
    }
}
