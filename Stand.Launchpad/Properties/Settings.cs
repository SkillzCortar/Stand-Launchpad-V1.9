// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.Properties.Settings
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Stand_Launchpad.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.5.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default => Settings.defaultInstance;

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("False")]
    public bool AutoInject
    {
      get => (bool) this[nameof (AutoInject)];
      set => this[nameof (AutoInject)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("")]
    public string CustomDll
    {
      get => (string) this[nameof (CustomDll)];
      set => this[nameof (CustomDll)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("False")]
    public bool Advanced
    {
      get => (bool) this[nameof (Advanced)];
      set => this[nameof (Advanced)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("1")]
    public string InjectDll
    {
      get => (string) this[nameof (InjectDll)];
      set => this[nameof (InjectDll)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("0")]
    public int AutoInjectDelaySeconds
    {
      get => (int) this[nameof (AutoInjectDelaySeconds)];
      set => this[nameof (AutoInjectDelaySeconds)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool MustUpgrade
    {
      get => (bool) this[nameof (MustUpgrade)];
      set => this[nameof (MustUpgrade)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("1")]
    public int GameLauncher
    {
      get => (int) this[nameof (GameLauncher)];
      set => this[nameof (GameLauncher)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("-1")]
    public int Version
    {
      get => (int) this[nameof (Version)];
      set => this[nameof (Version)] = (object) value;
    }
  }
}
