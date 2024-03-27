// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.Program
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

using System;
using System.Windows.Forms;

namespace Stand_Launchpad
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new Launchpad());
    }
  }
}
