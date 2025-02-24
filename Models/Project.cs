using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

public class Project
{
    [AllowNull]
    public int Id { get; set; }
    [AllowNull]
    public string Urn { get; set; }
    [AllowNull]
    public string Name { get; set; }
    [AllowNull]
    public DateTime CreatedAt { get; set; }
    [AllowNull]
    public DateTime UpdatedAt { get; set; }

}
