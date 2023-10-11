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
using VAPMAdapter.Catalog.POCO;
using CVEDetail = VAPMAdapter.OESIS.POCO.CVEDetail;

namespace VAPMAdapter.Catalog
{
    public class Catalog
    {
        private Products products;
        private CVES cves;
        private Patch_Aggregations patch_aggregations;
        private Patch_Associations patch_associations;
        private Vuln_Associations vuln_Associations;

        internal Products Products { get => products; set => products = value; }
        internal CVES Cves { get => cves; set => cves = value; }
        internal Patch_Aggregations Patch_Aggregations { get => patch_aggregations; set => patch_aggregations = value; }
        internal Patch_Associations Patch_Associations { get => patch_associations; set => patch_associations = value; }
        internal Vuln_Associations Vuln_Associations { get => vuln_Associations; set => vuln_Associations = value; }
        internal Vuln_Associations OS_Vuln_Associations { get => vuln_Associations; set => vuln_Associations = value; }

        private void CheckSuccess(bool result)
        {
            if (!result)
            {
                throw new Exception("Failed to run routine");
            }

        }


        public bool Load(string catalogRoot)
        {
            bool result = false;

            if (Directory.Exists(catalogRoot))
            {
                try
                {
                    Products = new Products();
                    CheckSuccess(Products.Load(catalogRoot + "/products.json"));

                    Patch_Associations = new Patch_Associations();
                    CheckSuccess(Patch_Associations.Load(catalogRoot + "/patch_associations.json"));

                    Patch_Aggregations = new Patch_Aggregations();
                    CheckSuccess(Patch_Aggregations.Load(catalogRoot + "/patch_aggregation.json"));

                    Vuln_Associations = new Vuln_Associations();
                    CheckSuccess(Vuln_Associations.Load(catalogRoot + "/vuln_associations.json"));

                    Cves = new CVES();
                    CheckSuccess(Cves.Load(catalogRoot + "/cves.json"));

                    result = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load catalog: " + e);
                }
            }
            else
            {
                Console.WriteLine("Failed to find catalog directory. Please run 'catlog download --token %token%' ");
            }


            return result;
        }


        public List<CatalogProduct> GetProductList()
        {
            return Products.GetList();
        }

        public List<CatalogVulnerabilityAssociation> GetVulnerabilityAssociationList()
        {
            return Vuln_Associations.GetList();
        }

        public List<CatalogVulnerabilityAssociation> GetVulnerabilityAssociationFromProductId(string productID)
        {
            Dictionary<string, List<CatalogVulnerabilityAssociation>> vulDictionary = vuln_Associations.GetProductVulnerablityDictionary();

            if (vulDictionary != null && vulDictionary.ContainsKey(productID))
            {
                return vulDictionary[productID];
            }

            return new List<CatalogVulnerabilityAssociation>();
        }

        public CatalogOSSupport GetOSSupportForProductId(string productId)
        {
            CatalogOSSupport result = new CatalogOSSupport();
            List<CatalogVulnerabilityAssociation> vulList = GetVulnerabilityAssociationFromProductId(productId);

            if (vulList != null)
            {
                foreach (CatalogVulnerabilityAssociation current in vulList)
                {
                    int osType = int.Parse(current.Os_type);

                    switch (osType)
                    {
                        case 1:
                            {
                                result.Windows = true;
                                break;
                            }
                        case 2:
                            {
                                result.Linux = true;
                                break;
                            }
                        case 4:
                            {
                                result.Mac = true;
                                break;
                            }
                    }
                }
            }

            return result;
        }


        public void PopulateSignatureVulnerability()
        {
            Dictionary<string, CatalogProduct> productDictionary = Products.GetProductIdDictionary();
            Dictionary<string, List<CatalogVulnerabilityAssociation>> productVulnDictionary = Vuln_Associations.GetProductVulnerablityDictionary();

            foreach (CatalogProduct product in productDictionary.Values)
            {
                if (productVulnDictionary.ContainsKey(product.Id))
                {
                    foreach (CatalogSignature signature in product.SigList)
                    {
                        signature.CVEList = productVulnDictionary[product.Id];
                        signature.CVECount = signature.CVEList.Count;
                    }
                }
            }
        }


        public string GetCVEDetail(string CVE)
        {
            return Cves.GetCVEDetails(CVE);
        }

        public List<CatalogCVEDate> GetCVEsFromDate(DateTime date)
        {
            return Cves.GetCVEsFromDate(date);
        }

        public List<CVEDetail> GetCVEDetailsList(List<CatalogVulnerabilityAssociation> cveList)
        {
            List<CVEDetail> result = new List<CVEDetail>();

            foreach (CatalogVulnerabilityAssociation vulnAssociation in cveList)
            {
                string cveString = GetCVEDetail(vulnAssociation.Cve);
                dynamic jsonOut = JObject.Parse(cveString);

                var cveJson = jsonOut;
                CVEDetail cveDetail = new CVEDetail();
                cveDetail.cveId =   (string)cveJson.cve;
                cveDetail.description = (string)cveJson.description;
                cveDetail.opswatSeverity = (int)cveJson.severity_index;
                cveDetail.rawData = cveString;
                result.Add(cveDetail);
            }

            return result;
        }




    }
}
