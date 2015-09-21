using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayMedia.Integration.FrameworkService.Interfaces.Common;
using System.Xml;
using PayMedia.Integration.FrameworkService.Interfaces;

namespace Utilities.CA
{
    public interface IComponentContext
    {
        IMsgContext MessageContext { get; }
        XmlDocument AggregatedData { get; }
        long HistoryId { get; }
        IFComponent IFComponent { get; set; }

        XmlDocument DataAggregatedCache { get; }
        void WriteInfo(ILog log);
        void WriteAsyncInfo(ILog log, Dictionary<string, string> asyncContext);
        void WriteError(ILog log);
    }
}
