using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public interface IExpressionMetadata
    {
        string Key { get; set; }
        string Description { get; set; }
        string Filter { get; set; }
        string Name { get; set; }
        bool InheritDataType { get; set; }
    }
}
