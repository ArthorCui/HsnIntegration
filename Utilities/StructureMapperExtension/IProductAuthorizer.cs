using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.StructureMapperExtension
{
    public interface IProductAuthorizer
    {
        bool IsUserAuthorizedToAccessProduct(User currentUser, int id);
    }
}
