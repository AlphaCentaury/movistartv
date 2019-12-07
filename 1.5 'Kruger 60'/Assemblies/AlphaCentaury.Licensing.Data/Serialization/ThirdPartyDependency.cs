// Copyright (C) 2014-2019, GitHub/Codeplex user AlphaCentaury
// 
// All rights reserved, except those granted by the governing license of this software.
// See 'license.txt' file in the project root for complete license information.
// 
// http://www.alphacentaury.org/movistartv https://github.com/AlphaCentaury

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AlphaCentaury.Licensing.Data.Serialization
{
    [Serializable]
    public class ThirdPartyDependency: BaseLibrary
    {
        [XmlAttribute("type")]
        [DefaultValue(ThirdPartyDependencyType.Other)]
        public ThirdPartyDependencyType Type { get; set; }

        public string Description { get; set; }
    } // class ThirdPartyDependency
} // namespace