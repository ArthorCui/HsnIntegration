using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.StructureMapperExtension
{
    public class ProductSecurityDecorator : IProductRepository
    {
        private readonly IProductRepository _target;
        private readonly IProductAuthorizer _securityService;
        private readonly User _currentUser;

        public ProductSecurityDecorator(IProductRepository target, IProductAuthorizer securityService, User currentUser)
        {
            this._target = target;
            this._securityService = securityService;
            this._currentUser = currentUser;
        }

        public Product Find(int id)
        {
            if (_securityService.IsUserAuthorizedToAccessProduct(_currentUser, id))
            {
                return _target.Find(id);
            }
            else
            {
                throw new Exception("No authorized to access this product.");
            }
        }
    }
}
