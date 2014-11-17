using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace ServiceLibary
{
    [ServiceContract]
    public interface IProductService
    {
        [OperationContract]
        double GetPrice(Product product);
    }
}
