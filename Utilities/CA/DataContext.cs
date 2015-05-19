using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayMedia.Integration.FrameworkService.Interfaces.Common;
using System.Xml;
using PayMedia.Integration.FrameworkService.Interfaces;

namespace Utilities.CA
{
    public class DataContext : IDataContext
    {
        [ThreadStatic]
        public static IDataContext Current;

        public IMsgContext MessageContext
        {
            get { throw new NotImplementedException(); }
        }

        public XmlDocument AggregatedData
        {
            get { throw new NotImplementedException(); }
        }

        public long HistoryId
        {
            get { throw new NotImplementedException(); }
        }

        public IFComponent IFComponent
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsValidating
        {
            get { throw new NotImplementedException(); }
        }

        public System.Xml.XmlDocument DataAggregatedCache
        {
            get { throw new NotImplementedException(); }
        }

        public void WriteInfo(ILog log)
        {
            throw new NotImplementedException();
        }

        public void WriteError(ILog log)
        {
            throw new NotImplementedException();
        }
    }
}
