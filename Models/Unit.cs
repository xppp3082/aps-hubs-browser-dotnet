using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

public class Unit
{
    [Key]
    public int Id { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("unit_number")]
    public string UnitNumber { get; set; }

    [Column("floor")]
    public int Floor { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("customer_project_id")]
    public int? CustomerProjectId { get; set; }
}
