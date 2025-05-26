using System.Collections.Generic;

public class CheckedObject
    {
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public string SymbolName { get; set; }
        public List<int> DbIds { get; set; }
    }