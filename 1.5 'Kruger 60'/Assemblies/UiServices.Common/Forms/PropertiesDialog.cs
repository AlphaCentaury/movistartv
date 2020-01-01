// Copyright (C) 2014-2019, GitHub/Codeplex user AlphaCentaury
// 
// All rights reserved, except those granted by the governing license of this software.
// See 'license.txt' file in the project root for complete license information.
// 
// http://www.alphacentaury.org/movistartv https://github.com/AlphaCentaury

using IpTviewr.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Property = System.Collections.Generic.KeyValuePair<string, string>;

namespace IpTviewr.UiServices.Common.Forms
{
    public partial class PropertiesDialog : Form
    {
        public ICollection<Property> ItemProperties { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public Image ItemIcon { get; set; }

        public PropertiesDialog()
        {
            InitializeComponent();
        } // constructor

        private void PropertiesDialog_Load(object sender, EventArgs e)
        {
            AppTelemetry.FormEvent(AppTelemetry.LoadEvent, this, Caption);
            Text = Caption;
            labelDescription.Text = (Description ?? Properties.PropertiesDialog.CaptionDefault);
            pictureBoxItemIcon.Image = ItemIcon;
            propertiesViewer1.Properties = ItemProperties;
        } // PropertiesDialog_Load
    } // class PropertiesDlg
} // namespace
