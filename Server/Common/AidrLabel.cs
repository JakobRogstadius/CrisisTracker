using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrisisTracker.Common
{
    public class AidrLabel
    {
        public int? AttributeID { get; set; }
        public string AttributeCode { get; set; }
        public string AttributeName { get; set; }
        
        public int? LabelID { get; set; }
        public string LabelCode { get; set; }
        public string LabelName { get; set; }

        public double Confidence { get; set; }
    }
}
