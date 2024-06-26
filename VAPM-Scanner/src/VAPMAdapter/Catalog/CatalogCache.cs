using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using System.IO;

namespace VAPMAdapter.Catalog.POCO
{
    public static class CatalogCache
    {
        private static readonly string FilePath = "catalog.bin";
        private static List<CatalogProduct> _cachedCatalog = null;

        public static List<CatalogProduct> CachedCatalog
        {
            get
            {
                if (_cachedCatalog == null && File.Exists(FilePath))
                {
                    // Load catalog from file
                    var bytes = File.ReadAllBytes(FilePath);
                    _cachedCatalog = MessagePackSerializer.Deserialize<List<CatalogProduct>>(bytes);
                }
                return _cachedCatalog;
            }
            set
            {
                _cachedCatalog = value;
                // Save catalog to file
                var bytes = MessagePackSerializer.Serialize(_cachedCatalog);
                File.WriteAllBytes(FilePath, bytes);
            }
        }
    }
}
