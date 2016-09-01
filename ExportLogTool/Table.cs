using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportLogTool
{
    public class Table
    {
        public string Name { get; set; }
        public bool HasPartition { get; set; }
        public TablePartition Partition { get; set; }
    }
}
