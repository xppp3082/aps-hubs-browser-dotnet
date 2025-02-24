using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChangeDetail
{
    public int Id { get; set; }
    public int DbId { get; set; }
    public int ElementId { get; set; }
    public ChangeDetailStatus Status { get; set; }
    public string Vector { get; set; }

    public ViewerUnit Unit { get; set; }
    public int UnitId { get; set; }
}
