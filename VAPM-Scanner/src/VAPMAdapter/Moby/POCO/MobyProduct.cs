﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Moby.POCO
{
    public class MobyProduct
    {
        public string Id;
        public string name;
        public string osType;
        public List<MobySignature> sigList;
        public bool cveDetection;
    }
}
