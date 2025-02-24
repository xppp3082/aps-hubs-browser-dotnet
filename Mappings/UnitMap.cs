using Dapper.FluentMap.Mapping;

public class UnitMap : EntityMap<Unit>
{
    public UnitMap()
    {
        Map(p => p.ProjectId).ToColumn("project_id");
        Map(p => p.UnitNumber).ToColumn("unit_number");
        Map(p => p.CustomerId).ToColumn("customer_id");
        Map(p => p.CreatedAt).ToColumn("created_at");
        Map(p => p.UpdatedAt).ToColumn("updated_at");
        Map(p => p.CustomerProjectId).ToColumn("customer_project_id");
    }
}
