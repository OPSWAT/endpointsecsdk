///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the
///  Compliance capability
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.OESIS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OPSWAT_Adapter.Tasks
{
    /// <summary>
    /// One detected application mapped to a single category. A product that belongs to several
    /// categories produces several ProductCategory rows.
    /// </summary>
    public class ProductCategory
    {
        public string application;
        public int signatureId;
        public string category;
    }

    /// <summary>
    /// Detects installed products and flattens them to (application, signature id, category) rows.
    /// </summary>
    public class TaskCategories
    {
        /// <summary>
        /// Returns one row per (product, category). A product with multiple categories is
        /// duplicated once per category; a product with none yields a single row with an empty
        /// category.
        /// </summary>
        public static List<ProductCategory> GetCategories()
        {
            List<ProductCategory> result = new List<ProductCategory>();

            OESISFramework.InitializeFramework();
            try
            {
                string json_product;
                // Include winget-sourced applications, not just OPSWAT-curated products.
                OESISCore.DetectAllProductsIncludingWinget(out json_product);
                JArray products = OESISCore.GetProductArrayFromString(json_product);

                foreach (JObject product in products)
                {
                    string name = (string)product["sig_name"];
                    // GetProductArrayFromString normalizes signature to an int (0 when absent,
                    // e.g. for some winget-sourced products).
                    int sig = (int)product["signature"];
                    JArray categories = (JArray)product["categories"];

                    bool added = false;
                    if (categories != null)
                    {
                        foreach (JToken categoryToken in categories)
                        {
                            // Only bare integer category ids are meaningful; skip anything else.
                            if (categoryToken.Type != JTokenType.Integer)
                            {
                                continue;
                            }

                            result.Add(new ProductCategory
                            {
                                application = name,
                                signatureId = sig,
                                category = CategoryName((int)categoryToken)
                            });
                            added = true;
                        }
                    }

                    // A product with no (usable) category still appears once with an empty category.
                    if (!added)
                    {
                        result.Add(new ProductCategory { application = name, signatureId = sig, category = "" });
                    }
                }
            }
            finally
            {
                OESISFramework.TearDown();
            }

            return result;
        }

        // Maps an OESIS category id to its friendly enum name, falling back to the numeric id.
        private static string CategoryName(int categoryId)
        {
            if (Enum.IsDefined(typeof(OESISCategory), categoryId))
            {
                return ((OESISCategory)categoryId).ToString();
            }
            return categoryId.ToString();
        }
    }
}
