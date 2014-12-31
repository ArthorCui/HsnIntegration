using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.StructureMapperExtension
{
    public class ProductRepoLogger : IProductRepository
    {
        private readonly IProductRepository _target;
        private readonly User _user;

        public ProductRepoLogger(IProductRepository target, User user)
        {
            this._target = target;
            this._user = user;
        }

        public Product Find(int id)
        {
            Console.WriteLine("{0} is requesting product {1}.", _user, id);
            return _target.Find(id);
        }
    }
}
