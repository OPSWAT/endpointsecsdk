using System;
using System.Collections.Generic;
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
                // Save catalog to file. WriteAllBytes stamps catalog.bin with the real save time,
                // which is what IsJsonCatalogChanged compares against products.json - so the cache
                // is correctly reused until products.json is refreshed.
                var bytes = MessagePackSerializer.Serialize(_cachedCatalog);
                File.WriteAllBytes(FilePath, bytes);
            }
        }
    }
}
