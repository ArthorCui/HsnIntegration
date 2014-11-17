using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceLibary
{
    public class ProductService : IProductService
    {
        public double GetPrice(Product product)
        {
            return product.Price;
        }
    }
}
