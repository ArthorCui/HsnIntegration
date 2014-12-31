using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Utilities.StructureMapperExtension
{
    public class ProductCachingDecorator : IProductRepository
    {
        private readonly IProductRepository _innerRepo;
        private readonly ConcurrentDictionary<int, Product> _cache;

        public ProductCachingDecorator(IProductRepository innerRepo)
        {
            _innerRepo = innerRepo;
            _cache = new ConcurrentDictionary<int, Product>();
        }

        public Product Find(int id)
        {
            return _cache.GetOrAdd(id, _ => _innerRepo.Find(id));
        }
    }
}
