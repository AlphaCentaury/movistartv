﻿// Copyright (C) 2014-2017, GitHub/Codeplex user AlphaCentaury
// 
// All rights reserved, except those granted by the governing license of this software.
// See 'license.txt' file in the project root for complete license information.
// 
// http://movistartv.alphacentaury.org/ https://github.com/AlphaCentaury

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IpTviewr.UiServices.Discovery.BroadcastList
{
    public class ListStatusChangedEventArgs: EventArgs
    {
        public ListStatusChangedEventArgs()
        {
            // no op
        } // constructor

        public ListStatusChangedEventArgs(bool hasItems)
        {
            HasItems = hasItems;
        } // constructor

        public bool HasItems
        {
            get;
            set;
        } // HasItems
    } // class ListStatusChangedEventArgs
} // namespace