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


        /// <summary>
        /// Reads "products.json" as a JObject. Throws exception if unsuccessful.
        /// </summary>
        /// <returns>A JObject representing the parsed JSON content.</returns>
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

        /// <summary>
        /// Extracts the JSON content under the "products" header from a provided JObject.
        /// This function assumes the JObject is retrieved using `getProductJsonObject`.
        /// Otherwise, the function returns null.
        /// </summary>
        /// <returns>A JObject containing product data if found under the "products" header, otherwise null.</returns>
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
        /// <summary>
        /// This function prioritizes returning the existing productList if it's not null.
        /// If productList is null, it retrieves a JObject from getProductsListJson().
        /// It then iterates through the JObject properties, creating `CatalogProduct` objects for each product:
        ///   - Extracts values (Id, Name, Vendor) from the JSON content.
        ///   - Creates a nested loop to process the "signatures" array within each product.
        ///     - For each signature, it creates a `CatalogSignature` object (FreshInstall, Name, Id).
        ///   - Adds all signatures to the product's `SigList` field.
        /// Finally, the function adds each `CatalogProduct` to a result list and returns the list.
        /// </summary>
        /// <returns>A list of CatalogProduct objects representing products, or empty list if retrieval fails.</returns>
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
                    newSignature.FreshInstall = false;

                    newSignature.Name = (string)currentSig["name"];
                    newSignature.Id = (string)currentSig["id"];

                    string stringFreshInstall = (string)currentSig["fresh_installable"];
                    if(stringFreshInstall == "1")
                    {
                        newSignature.FreshInstall = true;
                    }

                    newSignature.Platform = "Windows";
                    int signatureIntId = int.Parse(newSignature.Id);
                    if(signatureIntId > 100000)
                    {
                        if(signatureIntId > 200000)
                        {
                            newSignature.Platform = "Linux";
                        }
                        else
                        {
                            newSignature.Platform = "Mac";
                        }
                    }


                    if (newProduct.SupportsInstall)
                    {
                        if (currentSig["background_patching"] != null && (int)currentSig["background_patching"] > 0)
                        {
                            newSignature.BackgroundInstallSupport = true;
                        }

                        if (currentSig["validation_supported"] != null && (int)currentSig["validation_supported"] > 0)
                        {
                            newSignature.ValidateInstallSupport = true;
                        }
                    }





                    newProduct.SigList.Add(newSignature);
                }

                result.Add(newProduct);
            }

            productList = result;

            return result;
        }

        // Note this is not Thread Safe
        /// <summary> 
        /// This function prioritizes returning the existing productDictionary if it's not null.
        /// If productDictionary is null, it assumes GetList() retrieves a list of CatalogProduct objects.
        /// It then iterates through the product list, creating a dictionary that maps product IDs to the corresponding CatalogProduct objects.
        /// Finally, the function returns the constructed dictionary.
        /// </summary>
        /// <returns>A dictionary mapping product IDs to CatalogProduct objects, or empty dictionary if retrieval fails.</returns>
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
