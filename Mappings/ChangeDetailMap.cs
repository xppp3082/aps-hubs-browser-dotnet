using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.FluentMap.Mapping;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

public class ChangeDetailMap : EntityMap<ChangeDetail>
{
    public ChangeDetailMap()
    {
        Map(p => p.Id).ToColumn("id");
        Map(p => p.DbId).ToColumn("db_id");
        Map(p => p.ElementId).ToColumn("element_id");
        Map(p => p.Status).ToColumn("status");
        Map(p => p.Vector).ToColumn("vector");
        Map(p => p.UnitId).ToColumn("unit_id");
        Map(p => p.Unit).ToColumn("unit");
    }
}
