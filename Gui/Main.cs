﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using RssReaderFs;

namespace Gui
{
    public partial class Main : Form
    {
        Domain.RssReader reader_;

        delegate void ShowItemDelegate(Domain.RssItem item);

        public Main()
        {
            InitializeComponent();

            var path = @"feeds.json";

            // Load
            var sourcesOpt = Rss.Serialize.load(path);
            var sources    = (Domain.RssSource[])null;
            if ( sourcesOpt == null ) {
                this.ShowWarning("feed ファイルが読み込まれませんでした。");
                sources = new Domain.RssSource[] { };
            } else {
                sources = sourcesOpt.Value;
            }

            // Generate initial state
            this.reader_ = RssReader.Create(sources);

            // Subscribe feeds
            var subscriber = new MyRssSubscriber(
                new Action<Domain.RssItem[]>(async items => {
                    var len = items.Length;
                    if ( len == 0 ) return;
                    var msg =
                        String.Format("New {0} feeds available!", len);
                    this.ShowBalloonTip(msg, ToolTipIcon.Info);
                    foreach ( var item in items ) {
                        Invoke(new ShowItemDelegate(this.ShowItem), item);
                        await Task.Delay(3000);
                    }
                }));
            this.reader_ = RssReader.Subscribe(subscriber, this.reader_);

            // Start updating
            this.UpdateRssAsync();
        }

        private async Task UpdateRssAsync()
        {
            while ( true ) {
                var newReader = await RssReader.UpdateAll(this.reader_);
                this.reader_ = newReader;
                await Task.Delay(5 * 60 * 1000);
            }
        }

        private void ShowBalloonTip(string text, ToolTipIcon icon)
        {
            notifyIcon.ShowBalloonTip
                (1000
                , "RssReaderFs"
                , text
                , icon);
        }

        private void ShowWarning(string text)
        {
            this.ShowBalloonTip(text, ToolTipIcon.Warning);
        }

        private void ShowItem(Domain.RssItem item)
        {
            var srcOpt = RssReader.TryFindSource(item.Uri, this.reader_);
            if ( srcOpt == null ) {
                labelFeedSource.Text = "(unknown source)";
            } else {
                labelFeedSource.Text = srcOpt.Value.Name;
            }

            textBoxFeedTitle.Text = item.Title;
            textBoxFeedDesc.Text =
                (item.Desc == null)
                ? "(no description)"
                : item.Desc.Value;
        }
    }
}
