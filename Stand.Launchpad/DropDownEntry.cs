// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.DropDownEntry
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

namespace Stand_Launchpad
{
  internal class DropDownEntry
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public DropDownEntry(int id, string name)
    {
      this.Id = id;
      this.Name = name;
    }
  }
}
