using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportLogTool
{
    public class TablePartition
    {
        public string Name { get; set; }
        public bool HasSubPartition { get; set; }
        public TablePartition SubPartition { get; set; }
    }
}
