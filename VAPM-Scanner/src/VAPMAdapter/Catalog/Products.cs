///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAPMAdapter.Catalog.POCO;

namespace VAPMAdapter.Catalog
{
    internal class Products
    {
        private string jsonLocation;
        private List<CatalogProduct> productList;
        private Dictionary<string, CatalogProduct> productDictionary;


        public bool Load(string fileLocation)
        {
            bool result = true;
            jsonLocation = fileLocation;
            return result;
        }

        private JObject getProductJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Product Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }


        private JObject getProductsListJson()
        {
            JObject result = null;

            JObject productJsonObject = getProductJsonObject();
            JArray oesisJson = (JArray)productJsonObject["oesis"];

            foreach (JObject current in oesisJson.Children<JObject>())
            {
                if ("products" == JsonUtil.GetJObjectName(current))
                {
                    return ((JObject)current["products"]);
                }
            }

            return result;
        }


        // Note this is not thread safe
        public List<CatalogProduct> GetList()
        {
            // Return the cache version
            if (productList != null)
            {
                return productList;
            }

            List<CatalogProduct> result = new List<CatalogProduct>();

            JObject jsonProductList = (JObject)getProductsListJson();

            foreach (JProperty current in jsonProductList.Properties())
            {
                CatalogProduct newProduct = new CatalogProduct();
                newProduct.Vendor = new CatalogVendor();
                newProduct.SigList = new List<CatalogSignature>();

                newProduct.Id = current.Name;
                newProduct.Name = (string)jsonProductList[current.Name]["product"]["name"];
                newProduct.Vendor.Name = (string)jsonProductList[current.Name]["vendor"]["name"];
                newProduct.Vendor.Id = (string)jsonProductList[current.Name]["vendor"]["id"];

                newProduct.SupportsInstall = (bool)jsonProductList[current.Name]["support_3rd_party_patch"];

                JArray signatures = (JArray)jsonProductList[current.Name]["signatures"];
                foreach (JObject currentSig in signatures.Children<JObject>())
                {
                    CatalogSignature newSignature = new CatalogSignature();
                    newSignature.Name = (string)currentSig["name"];
                    newSignature.Id = (string)currentSig["id"];

                    newProduct.SigList.Add(newSignature);
                }

                result.Add(newProduct);
            }

            productList = result;

            return result;
        }

        // Note this is not Thread Safe
        public Dictionary<string, CatalogProduct> GetProductIdDictionary()
        {
            if (productDictionary != null)
            {
                return productDictionary;
            }

            Dictionary<string, CatalogProduct> newProductDictionary = new Dictionary<string, CatalogProduct>();

            List<CatalogProduct> productList = GetList();
            foreach (CatalogProduct product in productList)
            {
                newProductDictionary.Add(product.Id, product);
            }

            productDictionary = newProductDictionary;

            return newProductDictionary;
        }


        // What if old product is on the system?
        public List<CatalogSignature> GetSigsAsProducts(List<CatalogSignature> sigList)
        {
            Dictionary<string, CatalogSignature> resultDictionary = new Dictionary<string, CatalogSignature>();

            if (sigList != null)
            {
                foreach (CatalogSignature current in sigList)
                {
                    if (!resultDictionary.ContainsKey(current.Name))
                    {
                        resultDictionary.Add(current.Name, current);
                    }
                    else
                    {
                        CatalogSignature oldSig = resultDictionary[current.Name];
                        int oldID = int.Parse(oldSig.Id);
                        int newID = int.Parse(current.Id);

                        if (newID > oldID)
                        {
                            resultDictionary[current.Name] = current;
                        }
                    }
                }
            }


            return resultDictionary.Values.ToList<CatalogSignature>();
        }
    }
}
