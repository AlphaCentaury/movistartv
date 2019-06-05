// Copyright (C) 2014-2019, GitHub/Codeplex user AlphaCentaury
// 
// All rights reserved, except those granted by the governing license of this software.
// See 'license.txt' file in the project root for complete license information.
// 
// http://www.alphacentaury.org/movistartv https://github.com/AlphaCentaury

using Microsoft.SqlServer.MessageBox;
using IpTviewr.ChannelList.Properties;
using IpTviewr.Common;
using IpTviewr.Common.Telemetry;
using IpTviewr.Services.Record;
using IpTviewr.Services.Record.Serialization;
using IpTviewr.UiServices.Common.Forms;
using IpTviewr.UiServices.Common.Start;
using IpTviewr.UiServices.Configuration;
using IpTviewr.UiServices.Configuration.Logos;
using IpTviewr.UiServices.Configuration.Schema2014.Config;
using IpTviewr.UiServices.Discovery;
using IpTviewr.UiServices.Discovery.BroadcastList;
//using IpTviewr.UiServices.EPG;
using IpTviewr.UiServices.Forms;
using IpTviewr.UiServices.Record;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using IpTviewr.Core.IpTvProvider;
using IpTviewr.UiServices.EPG;
using IpTviewr.Services.EpgDiscovery;
//using IpTviewr.Core.IpTvProvider.EPG;

namespace IpTviewr.ChannelList
{
    public sealed partial class ChannelListForm : CommonBaseForm, ISplashScreenAwareForm
    {
        private class Notification
        {
            public string Text { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTime Displayed { get; set; }
        } // class Notification

        private const int ListObsoleteAge = 30;
        private const int ListOldAge = 15;
        private UiServiceProvider _selectedServiceProvider;
        private UiBroadcastDiscovery _broadcastDiscovery;
        private MulticastScannerDialog _multicastScanner;
        private UiBroadcastListManager _listManager;
        private Stack<Notification> _notifications;
        private EpgDataStore _epgDataStore;
        private CancellationTokenSource _tokenSource;

        // disabled functionality
        private const bool enable_menuItemDvbRecent = false;
        private const bool enable_menuItemDvbPackages = false;
        private const bool enable_menuItemDvbExport = false;
        private const bool enable_menuItemChannelFavorites = false;
        private const bool enable_menuItemChannelEditList = false;
        private bool enable_Epg = false;

        public ChannelListForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.IPTV;
            _notifications = new Stack<Notification>();
        } // constructor

        #region ISplashScreenAwareForm implementation

        public event EventHandler FormLoadCompleted;

        #endregion

        #region CommonBaseForm implementation

        protected override void ExceptionHandler(CommonBaseForm form, ExceptionEventData ex)
        {
            MyApplication.HandleException(form, ex);
        } // ExceptionHandler

        #endregion

        #region Form event handlers

        private void ChannelListForm_Load(object sender, EventArgs e)
        {
            if (!SafeCall(ChannelListForm_Load_Implementation, sender, e))
            {
                this.Close();
            } // if
        }  // ChannelListForm_Load

        private void ChannelListForm_Shown(object sender, EventArgs e)
        {
            BasicGoogleTelemetry.SendScreenHit(this, "Shown");
            if (_selectedServiceProvider == null)
            {
                SafeCall(SelectProvider);
            } // if
        } // ChannelListForm_Shown

        private void ChannelListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // can't close the form if a services scan is in progress; the user must manually cancel it first
            e.Cancel = IsScanActive();
        } // ChannelListForm_FormClosing

        #endregion

        #region Form event handlers implementation

        private void ChannelListForm_Load_Implementation(object sender, EventArgs e)
        {
            BasicGoogleTelemetry.SendScreenHit(this, "Load");

            Text = Properties.Texts.AppCaption;

            // disable functionality
            menuItemDvbRecent.Enabled = enable_menuItemDvbRecent;
            menuItemDvbPackages.Enabled = enable_menuItemDvbPackages;
            menuItemDvbExport.Enabled = enable_menuItemDvbExport;

            var settings = UiBroadcastListSettingsRegistration.Settings;
            _listManager = new UiBroadcastListManager(listViewChannelList, settings, imageListChannels, imageListChannelsLarge, true);
            _listManager.SelectionChanged += ListManager_SelectionChanged;
            _listManager.StatusChanged += ListManager_StatusChanged;

            SetupContextMenuList();

            // Empty notifications
            Notify(null, null, -1);

            // set-up EPG functionality
            enable_Epg = AppUiConfiguration.Current.User.Epg.Enabled;
            epgMiniGuide.Visible = false;
            epgMiniGuide.IsDisabled = !enable_Epg;
            if (!enable_Epg)
            { 
                foreach (ToolStripItem item in menuItemEpg.DropDownItems)
                {
                    item.Enabled = false;
                } // foreach
            } // if

            // load from cache, if available
            _selectedServiceProvider = SelectProviderDialog.GetLastUserSelectedProvider(Properties.Settings.Default.LastSelectedServiceProvider);
            ServiceProviderChanged();

            // notify Splash Screeen the form has finished loading and is about to be shown
            FormLoadCompleted?.Invoke(this, e);
        } // ChannelListForm_Load_Implementation

        #endregion

        #region 'IPTV > Package' menu event handlers

        private void menuItemPackagesSelect_Click(object sender, EventArgs e)
        {
            SafeCall(Implementation_menuItemPackagesSelect_Click, sender, e);
        } // menuItemPackagesSelect_Click

        private void menuItemPackagesManage_Click(object sender, EventArgs e)
        {
            SafeCall(Implementation_menuItemPackagesManage_Click, sender, e);
        } // menuItemPackagesManage_Click

        #endregion

        #region 'IPTV > Package' menu event handlers implementation

        private void Implementation_menuItemPackagesSelect_Click(object sender, EventArgs e)
        {
            NotImplementedBox.ShowBox(this, "menuItemPackagesSelect");
        } // Implementation_menuItemPackagesSelect_Click

        private void Implementation_menuItemPackagesManage_Click(object sender, EventArgs e)
        {
            NotImplementedBox.ShowBox(this, "menuItemPackagesManage");
        } // Implementation_menuItemPackagesManage_Click

        #endregion

        #region Auxiliary methods: common

        private void ExceptionHandler(string message, Exception ex)
        {
            MyApplication.HandleException(this, message, ex);
        } // ExceptionHandler

        private void Notify(Image icon, string text, int dismissTime)
        {
            timerDismissNotification.Enabled = false;

            if (statusLabelMain.Image != null)
            {
                statusLabelMain.Image.Dispose();
            } // if

            statusLabelMain.Image = icon;
            statusLabelMain.Text = text ?? "Ready";

            if ((text != null) && (dismissTime > 0))
            {
                timerDismissNotification.Interval = dismissTime;
                timerDismissNotification.Enabled = true;
            } // if
        } // Notify

        private void timerDismissNotification_Tick(object sender, EventArgs e)
        {
            try
            {
                Notify(null, null, 0);
            }
            catch
            {
                timerDismissNotification.Enabled = false;
            } // try-catch
        } // timerDismissNotification_Tick

        private void OpenUrl(string url)
        {
            Launcher.OpenUrl(this, url, HandleException, Properties.Texts.OpenUrlError);
        } // OpenUrl

        #endregion

        #region WORK IN PROGRESS - EXPERIMENTS - Code to clean-up and/or move to appropriate sections

        private void SetFullscreenMode(bool fullScreen, bool topMost)
        {
            if (fullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = topMost;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.TopMost = false;
            } // if-else
        } // SetFullscreenMode

        private void menuItemChannelFavorites_Click(object sender, EventArgs e)
        {
            NotImplementedBox.ShowBox(this, "menuItemChannelFavorites");
        }  // menuItemChannelFavorites_Click

        private void menuItemChannelFavoritesEdit_Click(object sender, EventArgs e)
        {
            NotImplementedBox.ShowBox(this, "menuItemChannelFavorites");
        }  // menuItemChannelFavoritesEdit_Click

        #endregion
    } // class ChannelListForm
} // namespace
