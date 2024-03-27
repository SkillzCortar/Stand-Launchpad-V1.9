// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.Changelog
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Stand_Launchpad
{
  public class Changelog : Form
  {
    private IContainer components;
    private WebBrowser webBrowser1;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
      IntPtr hwnd,
      int attr,
      int[] attrValue,
      int attrSize);

    public Changelog()
    {
      this.InitializeComponent();
      if (Changelog.DwmSetWindowAttribute(this.Handle, 19, new int[1]
      {
        1
      }, 4) == 0)
        return;
      Changelog.DwmSetWindowAttribute(this.Handle, 20, new int[1]
      {
        1
      }, 4);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.webBrowser1 = new WebBrowser();
      this.SuspendLayout();
      this.webBrowser1.Dock = DockStyle.Fill;
      this.webBrowser1.Location = new Point(0, 0);
      this.webBrowser1.MinimumSize = new Size(20, 20);
      this.webBrowser1.Name = "webBrowser1";
      this.webBrowser1.Size = new Size(800, 450);
      this.webBrowser1.TabIndex = 0;
      this.webBrowser1.Url = new Uri("https://stand.gg/help/changelog-launchpad", UriKind.Absolute);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(800, 450);
      this.Controls.Add((Control) this.webBrowser1);
      this.Name = nameof (Changelog);
      this.Text = nameof (Changelog);
      this.Icon = (Icon) new ComponentResourceManager(typeof (Launchpad)).GetObject("$this.Icon");
      this.ResumeLayout(false);
    }
  }
}
