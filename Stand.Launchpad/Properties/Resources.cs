// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.Properties.Resources
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Stand_Launchpad.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (Stand_Launchpad.Properties.Resources.resourceMan == null)
          Stand_Launchpad.Properties.Resources.resourceMan = new ResourceManager("Stand_Launchpad.Properties.Resources", typeof (Stand_Launchpad.Properties.Resources).Assembly);
        return Stand_Launchpad.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => Stand_Launchpad.Properties.Resources.resourceCulture;
      set => Stand_Launchpad.Properties.Resources.resourceCulture = value;
    }
  }
}
