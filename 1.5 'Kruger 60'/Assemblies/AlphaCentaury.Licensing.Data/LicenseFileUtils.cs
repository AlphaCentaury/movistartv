// Copyright (C) 2014-2019, GitHub/Codeplex user AlphaCentaury
// 
// All rights reserved, except those granted by the governing license of this software.
// See 'license.txt' file in the project root for complete license information.
// 
// http://www.alphacentaury.org/movistartv https://github.com/AlphaCentaury

using System;
using System.Collections.Generic;
using System.Linq;
using AlphaCentaury.Licensing.Data.Serialization;

namespace AlphaCentaury.Licensing.Data
{
    public static class LicensingDataTools
    {
        public static void ExpandDependencies(this List<LicensingFile> files)
        {
            var expander = new ExpandDependencies(files);
            expander.Expand();
        } // FirstPass
    } // class LicenseFileUtils
} // namespace