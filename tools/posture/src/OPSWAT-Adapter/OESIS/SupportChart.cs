///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ComplianceAdapater.OESIS
{
    public class SupportChart
    {
        private static string getSupportFile(string filePrefix)
        {
            string result = null;
            DirectoryInfo dirInfo = new DirectoryInfo(".");
            FileInfo[] files = dirInfo.GetFiles();
            
            foreach(FileInfo fi in files)
            {
                if(fi.Name.StartsWith(filePrefix))
                {
                    result = fi.Name;
                    break;
                }
            }

            return result;
        }



        public static List<ProductInfo> LoadProductList(OESISCategory category)
        {
            List<ProductInfo> result = new List<ProductInfo>();

            if(category == OESISCategory.FIREWALL)
            {
                result = ParseXML(getSupportFile("Windows_FIREWALL"));
            }
            else if (category == OESISCategory.DISK_ENCRYPTION)
            {
                result = ParseXML(getSupportFile("Windows_ENCRYPTION"));
            }
            else if (category == OESISCategory.ANTIMALWARE)
            {
                result = ParseXML(getSupportFile("Windows_ANTIMALWARE"));
            }

            return result;
        }


        private static XmlElement getTableElement(XmlDocument dom)
        {
            XmlElement result = null;

            foreach (XmlNode stylesheetRoot in dom.ChildNodes)
            {
                if (stylesheetRoot.Name == "xsl:stylesheet")
                {
                    foreach (XmlNode variableRoot in stylesheetRoot.ChildNodes)
                    {
                        if (variableRoot.Name == "xsl:variable")
                        {
                            foreach (XmlNode supportChartNode in variableRoot.ChildNodes)
                            {
                                if (supportChartNode.Name == "supportchart")
                                {
                                    foreach (XmlNode tableNode in supportChartNode.ChildNodes)
                                    {
                                        if (tableNode.Name == "Table")
                                        {
                                            return (XmlElement)tableNode;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }


        private static List<ProductInfo> ParseXML(string filePath)
        {
            List<ProductInfo> result = new List<ProductInfo>();
            SortedDictionary<string, ProductInfo> sortedResult = new SortedDictionary<string, ProductInfo>();
            XmlDocument dom = new XmlDocument();
            dom.Load(filePath);

            XmlElement tableElement = getTableElement(dom);
            if (tableElement != null)
            {
                foreach (XmlNode vendorNode in tableElement.ChildNodes)
                {
                    if (vendorNode.Name == "Vendor")
                    {
                        foreach (XmlElement productNode in vendorNode.ChildNodes)
                        {
                            string productName = productNode.GetAttribute("Product_Name");
                            int productSigId = int.Parse(productNode.GetAttribute("Signature_ID"));

                            if (!sortedResult.ContainsKey(productName))
                            {
                                ProductInfo newInfo = new ProductInfo();
                                newInfo.name = productName;
                                newInfo.sigId = productSigId;

                                sortedResult.Add(productName, newInfo);
                            }
                        }
                    }
                }
            }

            return new List<ProductInfo>(sortedResult.Values);
        }



    }
}
