using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportLogTool
{
    public class LogModel
    {
        public string TargetSchema { get; set; }

        public List<string> ObjectTypes { get; set; }

        public List<TableInfo> TableInfos { get; set; }

        public string DumpFileNamePath { get; set; }

        public string JobName { get; set; }

        public TimeSpan ElaspeTime { get; set; }
    }
}
