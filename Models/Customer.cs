using System;
using System.Diagnostics.CodeAnalysis;

public class Customer
{
    [AllowNull]
    public int Id { get; set; }
    [AllowNull]
    public string Name { get; set; }
    [AllowNull]
    public string Phone { get; set; }
    [AllowNull]
    public string Email { get; set; }
    [AllowNull]
    public DateTime CreatedAt { get; set; }
    [AllowNull]
    public DateTime UpdatedAt { get; set; }
}