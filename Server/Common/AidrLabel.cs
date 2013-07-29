/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

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
