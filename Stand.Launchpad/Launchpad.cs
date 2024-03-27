// Decompiled with JetBrains decompiler
// Type: Stand_Launchpad.Launchpad
// Assembly: Stand Launchpad, Version=1.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EAB18CAB-19C6-45C7-9D82-879DBD93998F
// Assembly location: S:\My Downloads\Stand.Launchpad.exe

using Microsoft.Win32;
using Stand_Launchpad.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stand_Launchpad
{
  public class Launchpad : Form
  {
    private static Random random = new Random();
    private const string launchpad_update_version = "1.9";
    private const string launchpad_display_version = "1.9";
    private string stand_dir;
    private FileStream lockfile;
    private string stand_dll;
    private const int width_simple = 248;
    private readonly int width_advanced;
    private string[] versions;
    private int download_progress;
    private int gta_pid;
    private bool game_was_open;
    private bool can_auto_inject = true;
    private bool any_successful_injection;
    private IContainer components;
    private Button InjectBtn;
    private Label InfoText;
    private CheckBox AutoInjectCheckBox;
    private OpenFileDialog CustomDllDialog;
    private Button AdvancedBtn;
    private System.Windows.Forms.Timer ProcessScanTimer;
    private Button RemoveBtn;
    private Button UpBtn;
    private Button DownBtn;
    private NumericUpDown AutoInjectDelaySeconds;
    private Label AutoInjectDelayLabel;
    private System.Windows.Forms.Timer AutoInjectTimer;
    private System.Windows.Forms.Timer GameClosedTimer;
    private System.Windows.Forms.Timer UpdateTimer;
    private Button ChanglogBtn;
    private System.Windows.Forms.Timer ReInjectTimer;
    private Button StandFolderBtn;
    private Button UpdCheckBtn;
    private ListView DllList;
    private ComboBox LauncherType;
    private BindingSource dropDownEntryBindingSource;
    private Button LaunchBtn;
    private ProgressBar progressBar1;
    private ColumnHeader Column;
    private Button AddBtn;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
      uint dwDesiredAccess,
      int bInheritHandle,
      uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("dwmapi.dll", SetLastError = true)]
    private static extern int DwmSetWindowAttribute(
      IntPtr hwnd,
      uint dwAttribute,
      int[] pvAttribute,
      uint cbAttribute);

    protected override void OnHandleCreated(EventArgs e)
    {
      if (Launchpad.DwmSetWindowAttribute(this.Handle, 19U, new int[1]
      {
        1
      }, 4U) == 0)
        return;
      Launchpad.DwmSetWindowAttribute(this.Handle, 20U, new int[1]
      {
        1
      }, 4U);
    }

    public Launchpad()
    {
      this.stand_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Stand";
      if (!Directory.Exists(this.stand_dir))
        Directory.CreateDirectory(this.stand_dir);
      if (!Directory.Exists(this.stand_dir + "\\Bin"))
        Directory.CreateDirectory(this.stand_dir + "\\Bin");
      if (System.IO.File.Exists(this.stand_dir + "\\Bin\\Launchpad.lock"))
      {
        try
        {
          System.IO.File.Delete(this.stand_dir + "\\Bin\\Launchpad.lock");
        }
        catch (Exception ex)
        {
          int num = (int) this.showMessageBox("Only one instance of the Launchpad can be open at a time.", icon: MessageBoxIcon.Hand);
          Environment.Exit(1);
          return;
        }
      }
      this.lockfile = System.IO.File.Create(this.stand_dir + "\\Bin\\Launchpad.lock");
      this.InitializeComponent();
      this.width_advanced = this.Width;
      this.Text += " 1.9";
      this.LauncherType.DataSource = (object) new DropDownEntry[3]
      {
        new DropDownEntry(1, "Steam"),
        new DropDownEntry(0, "Epic Games"),
        new DropDownEntry(2, "Rockstar Games")
      };
      try
      {
        int num = Settings.Default.MustUpgrade ? 1 : 0;
      }
      catch (Exception ex)
      {
        int num = (int) this.showMessageBox("Your Launchpad configuration seems to be corrupted. The easiest fix for this is just heading into %localappdata%\\Calamity,_Inc\\ and deleting the Launchpad folders.", icon: MessageBoxIcon.Hand);
        Environment.Exit(2);
        return;
      }
      if (Settings.Default.MustUpgrade)
      {
        Settings.Default.Upgrade();
        Settings.Default.MustUpgrade = false;
        if (Settings.Default.Version == -1)
          Settings.Default.Version = 0;
        Settings.Default.Save();
      }
      this.AutoInjectCheckBox.Checked = Settings.Default.AutoInject;
      this.AutoInjectDelaySeconds.Value = (Decimal) Settings.Default.AutoInjectDelaySeconds;
      this.LauncherType.SelectedValue = (object) Settings.Default.GameLauncher;
      this.toggleInjectOrLaunchBtn(false);
      this.UpdateTimer.Start();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
      this.UpdateTimer.Stop();
      this.checkForUpdate(false);
      this.updateGtaPid();
      this.processGtaPidUpdate(false);
      if (this.gta_pid != 0)
        this.InjectBtn.Focus();
      this.ProcessScanTimer.Start();
    }

    private bool isStandDll(FileInfo file) => file.Name.StartsWith("Stand ") && file.Name.EndsWith(".dll");

    private string getStandVersionFromDll(FileInfo file) => file.Name.Substring(6, file.Name.Length - 6 - 4);

    private int checkForUpdate(bool recheck)
    {
      Task<string> stringAsync = new HttpClient().GetStringAsync("https://stand.gg/versions.txt");
      DirectoryInfo directoryInfo = new DirectoryInfo(this.stand_dir + "\\Bin\\");
      string str = "";
      try
      {
        stringAsync.Wait();
        str = stringAsync.Result;
      }
      catch (Exception ex)
      {
        if (!recheck)
        {
          foreach (FileInfo file in directoryInfo.GetFiles())
          {
            if (this.isStandDll(file))
              str = "1.9:" + this.getStandVersionFromDll(file);
          }
        }
      }
      if (str.Length == 0)
      {
        int num = (int) this.showMessageBox("Failed to get version information. Ensure you're connected to the internet and have no antivirus program or firewall interfering.");
        if (recheck)
          return -1;
        Application.Exit();
      }
      this.versions = str.Split(':');
      if (recheck)
      {
        this.saveSettings();
        this.DllList.Items.Clear();
      }
      if (!Settings.Default.Advanced)
        this.updateAdvancedMode();
      this.DllList.Items.Add("Stand " + this.versions[1]);
      if (Settings.Default.CustomDll != "")
      {
        string customDll = Settings.Default.CustomDll;
        char[] chArray = new char[1]{ '|' };
        foreach (string text in customDll.Split(chArray))
          this.DllList.Items.Add(text);
      }
      for (int index = 0; index < Settings.Default.InjectDll.Length; ++index)
      {
        if (Settings.Default.InjectDll.Substring(index, 1) == "1")
          this.DllList.Items[index].Checked = true;
      }
      bool flag = false;
      this.stand_dll = this.stand_dir + "\\Bin\\Stand " + this.versions[1] + ".dll";
      if (!System.IO.File.Exists(this.stand_dll))
      {
        if (this.downloadStandDll())
        {
          foreach (FileInfo file in directoryInfo.GetFiles())
          {
            if (this.isStandDll(file))
            {
              if (this.getStandVersionFromDll(file) != this.versions[1])
              {
                try
                {
                  file.Delete();
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex.ToString());
                }
              }
            }
          }
        }
        flag = true;
      }
      if (this.versions[0] != "1.9")
      {
        if (this.showMessageBox("Launchpad " + this.versions[0] + " is available. Would you like to download it?", MessageBoxButtons.YesNo) == DialogResult.Yes)
          Process.Start("https://stand.gg/launchpad_update");
        flag = true;
      }
      return flag ? 1 : 0;
    }

    private void onDownloadProgress(object sender, DownloadProgressChangedEventArgs e) => this.download_progress = e.ProgressPercentage;

    private void onDownloadComplete(object sender, AsyncCompletedEventArgs e)
    {
      lock (e.UserState)
        Monitor.Pulse(e.UserState);
    }

    private bool downloadStandDll()
    {
      bool flag = true;
      this.InfoText.Text = "Downloading Stand " + this.versions[1] + "...";
      this.download_progress = 0;
      this.progressBar1.Show();
      Task task = Task.Run((Action) (() =>
      {
        WebClient webClient = new WebClient();
        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.onDownloadProgress);
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.onDownloadComplete);
        object userToken = new object();
        lock (userToken)
        {
          webClient.DownloadFileAsync(new Uri("https://stand.gg/Stand%20" + this.versions[1] + ".dll"), this.stand_dll + ".tmp", userToken);
          Monitor.Wait(userToken);
        }
      }));
      do
      {
        this.progressBar1.Value = this.download_progress;
      }
      while (!task.Wait(20));
      System.IO.File.Move(this.stand_dll + ".tmp", this.stand_dll);
      if (new FileInfo(this.stand_dll).Length < 1024L)
      {
        System.IO.File.Delete(this.stand_dll);
        int num = (int) this.showMessageBox("It looks like the DLL download has failed. Ensure you have no antivirus program interfering.");
        flag = false;
      }
      this.progressBar1.Hide();
      return flag;
    }

    private void ProcessScanTimer_Tick(object sender, EventArgs e)
    {
      if (!this.updateGtaPid())
        return;
      this.processGtaPidUpdate(this.can_auto_inject);
    }

    private bool updateGtaPid()
    {
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName == "GTA5")
        {
          if (this.gta_pid == process.Id)
            return false;
          this.gta_pid = process.Id;
          this.game_was_open = true;
          return true;
        }
      }
      this.AutoInjectTimer.Stop();
      int num = (uint) this.gta_pid > 0U ? 1 : 0;
      this.gta_pid = 0;
      return num != 0;
    }

    private void processGtaPidUpdate(bool proc_can_auto_inject)
    {
      bool gameRunning = (uint) this.gta_pid > 0U;
      this.toggleInjectOrLaunchBtn(gameRunning);
      if (gameRunning)
      {
        if (this.AutoInjectCheckBox.Checked & proc_can_auto_inject)
        {
          if (Settings.Default.Advanced && this.AutoInjectDelaySeconds.Value > 0M)
          {
            this.InfoText.Text = "Automatically injecting in a few seconds...";
            this.AutoInjectTimer.Interval = (int) this.AutoInjectDelaySeconds.Value * 1000;
            this.AutoInjectTimer.Start();
          }
          else
            this.inject();
        }
        else
          this.InfoText.Text = "Ready to inject.";
      }
      else
      {
        this.InfoText.Text = "Ready to inject; just start the game.";
        if (!this.game_was_open)
          return;
        this.game_was_open = false;
        this.can_auto_inject = false;
        this.GameClosedTimer.Start();
      }
    }

    private void InjectBtn_Click(object sender, EventArgs e) => this.inject();

    private void AutoInjectTimer_Tick(object sender, EventArgs e) => this.inject();

    private unsafe void inject()
    {
      bool flag = false;
      this.AutoInjectTimer.Stop();
      this.ProcessScanTimer.Stop();
      this.InjectBtn.Enabled = false;
      List<string> stringList = new List<string>();
      if (Settings.Default.Advanced)
      {
        for (int index = 0; index < this.DllList.Items.Count; ++index)
        {
          if (this.DllList.Items[index].Checked)
            stringList.Add(index == 0 ? this.stand_dll : this.DllList.Items[index].Text);
        }
      }
      else
        stringList.Add(this.stand_dll);
      if (stringList.Contains(this.stand_dll) && !System.IO.File.Exists(this.stand_dll) && !this.downloadStandDll())
        stringList.Remove(this.stand_dll);
      this.InfoText.Text = "Injecting...";
      int num1 = 0;
      IntPtr num2 = Launchpad.OpenProcess(1082U, 1, (uint) this.gta_pid);
      if (num2 == IntPtr.Zero)
      {
        Console.WriteLine("Failed to get a hold of the game's process.");
      }
      else
      {
        IntPtr moduleHandle = Launchpad.GetModuleHandle("kernel32.dll");
        IntPtr procAddress = Launchpad.GetProcAddress(moduleHandle, "LoadLibraryW");
        if (procAddress == IntPtr.Zero)
        {
          Console.WriteLine("Failed to find LoadLibraryW.");
        }
        else
        {
          string path = this.stand_dir + "\\Bin\\Temp";
          if (!Directory.Exists(path))
          {
            Directory.CreateDirectory(path);
          }
          else
          {
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
            {
              try
              {
                file.Delete();
              }
              catch (Exception ex)
              {
              }
            }
          }
          Launchpad.VirtualAllocExDelegate forFunctionPointer1 = (Launchpad.VirtualAllocExDelegate) Marshal.GetDelegateForFunctionPointer(Launchpad.GetProcAddress(moduleHandle, "VirtualAllocEx"), typeof (Launchpad.VirtualAllocExDelegate));
          Launchpad.WriteProcessMemoryDelegate forFunctionPointer2 = (Launchpad.WriteProcessMemoryDelegate) Marshal.GetDelegateForFunctionPointer(Launchpad.GetProcAddress(moduleHandle, "WriteProcessMemory"), typeof (Launchpad.WriteProcessMemoryDelegate));
          Launchpad.CreateRemoteThreadDelegate forFunctionPointer3 = (Launchpad.CreateRemoteThreadDelegate) Marshal.GetDelegateForFunctionPointer(Launchpad.GetProcAddress(moduleHandle, "CreateRemoteThread"), typeof (Launchpad.CreateRemoteThreadDelegate));
          try
          {
            foreach (string str1 in stringList)
            {
              if (!System.IO.File.Exists(str1))
              {
                Console.WriteLine("Couldn't inject " + str1 + " because the file doesn't exist.");
              }
              else
              {
                string str2 = path + "\\SL_" + Launchpad.generateRandomString(5) + ".dll";
                System.IO.File.Copy(str1, str2);
                byte[] bytes = Encoding.Unicode.GetBytes(str2);
                IntPtr num3 = forFunctionPointer1(num2, (IntPtr) (void*) null, (IntPtr) bytes.Length, 12288U, 64U);
                if (num3 == IntPtr.Zero)
                  Console.WriteLine("Couldn't allocate the bytes to represent " + str1);
                else if (forFunctionPointer2(num2, num3, bytes, (uint) bytes.Length, 0) == 0)
                  Console.WriteLine("Couldn't write " + str2 + " to allocated memory");
                else if (forFunctionPointer3(num2, (IntPtr) (void*) null, IntPtr.Zero, procAddress, num3, 0U, (IntPtr) (void*) null) == IntPtr.Zero)
                  Console.WriteLine("Failed to create remote thread for " + str1);
                else
                  ++num1;
              }
            }
          }
          catch (IOException ex)
          {
            this.Activate();
            flag = true;
            int num4 = (int) this.showMessageBox("Your antivirus seems to be preventing injection.\nDisable your antivirus or add an exclusion and try again.", icon: MessageBoxIcon.Hand);
          }
        }
        Launchpad.CloseHandle(num2);
      }
      this.InfoText.Text = "Injected " + num1.ToString() + "/" + stringList.Count.ToString() + " DLLs.";
      if (num1 == 0)
      {
        if (!this.any_successful_injection && stringList.Count != 0 && !flag)
        {
          int num5 = (int) this.showMessageBox("No DLL was injected. You may need to start the Launchpad as Administrator.");
        }
        this.EnableReInject();
      }
      else
      {
        this.any_successful_injection = true;
        this.ReInjectTimer.Start();
      }
    }

    private static string generateRandomString(int length) => new string(Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select<string, char>((Func<string, char>) (s => s[Launchpad.random.Next(s.Length)])).ToArray<char>());

    private DialogResult showMessageBox(
      string message,
      MessageBoxButtons buttons = MessageBoxButtons.OK,
      MessageBoxIcon icon = MessageBoxIcon.None)
    {
      return MessageBox.Show(message, "Stand Launchpad 1.9", buttons, icon);
    }

    private void Launchpad_FormClosing(object sender, FormClosingEventArgs e)
    {
      Settings.Default.AutoInject = this.AutoInjectCheckBox.Checked;
      Settings.Default.AutoInjectDelaySeconds = (int) this.AutoInjectDelaySeconds.Value;
      Settings.Default.GameLauncher = ((DropDownEntry) this.LauncherType.SelectedItem).Id;
      this.saveSettings();
      this.lockfile.Close();
      System.IO.File.Delete(this.stand_dir + "\\Bin\\Launchpad.lock");
    }

    private void saveSettings()
    {
      Settings.Default.InjectDll = "";
      Settings.Default.CustomDll = "";
      for (int index = 0; index < this.DllList.Items.Count; ++index)
      {
        Settings.Default.InjectDll += this.DllList.Items[index].Checked ? "1" : "0";
        if (index != 0)
        {
          // ISSUE: variable of a compiler-generated type
          Settings settings = Settings.Default;
          settings.CustomDll = settings.CustomDll + this.DllList.Items[index].Text + "|";
        }
      }
      if (Settings.Default.CustomDll != "")
        Settings.Default.CustomDll = Settings.Default.CustomDll.Substring(0, Settings.Default.CustomDll.Length - 1);
      Settings.Default.Save();
    }

    private void CustomDllDialog_FileOk(object sender, CancelEventArgs e) => this.addDll(this.CustomDllDialog.FileName);

    private void addDll(string path) => this.DllList.Items[this.DllList.Items.Add(path).Index].Checked = true;

    private void AdvancedBtn_Click(object sender, EventArgs e)
    {
      Settings.Default.Advanced = !Settings.Default.Advanced;
      this.updateAdvancedMode();
    }

    private void updateAdvancedMode()
    {
      if (Settings.Default.Advanced)
      {
        this.Width = this.width_advanced;
        this.MinimizeBox = true;
        this.InjectBtn.Text = "Inject";
        this.AutoInjectDelaySeconds.Visible = true;
        this.AddBtn.TabStop = true;
        this.RemoveBtn.TabStop = true;
        this.DllList.Visible = true;
      }
      else
      {
        this.MinimizeBox = false;
        this.Width = 248;
        this.InjectBtn.Text = "Inject Stand " + this.versions[1];
        this.AutoInjectDelaySeconds.Visible = false;
        this.AddBtn.TabStop = false;
        this.RemoveBtn.TabStop = false;
        this.DllList.Visible = false;
      }
    }

    private void AddBtn_Click(object sender, EventArgs e)
    {
      int num = (int) this.CustomDllDialog.ShowDialog();
    }

    private void RemoveBtn_Click(object sender, EventArgs e) => this.removeSelectedDll();

    private void UpBtn_Click(object sender, EventArgs e)
    {
      if (this.DllList.SelectedItems.Count != 1)
        return;
      int selectedIndex = this.DllList.SelectedIndices[0];
      if (selectedIndex <= 1)
        return;
      ListViewItem listViewItem = this.DllList.Items[selectedIndex];
      this.DllList.Items.RemoveAt(selectedIndex);
      this.DllList.Items.Insert(selectedIndex - 1, listViewItem);
      this.DllList.Items[selectedIndex - 1].Selected = true;
      this.saveSettings();
    }

    private void DownBtn_Click(object sender, EventArgs e)
    {
      if (this.DllList.SelectedItems.Count != 1)
        return;
      int selectedIndex = this.DllList.SelectedIndices[0];
      if (selectedIndex == 0 || selectedIndex >= this.DllList.Items.Count - 1)
        return;
      ListViewItem listViewItem = this.DllList.Items[selectedIndex];
      this.DllList.Items.RemoveAt(selectedIndex);
      this.DllList.Items.Insert(selectedIndex + 1, listViewItem);
      this.DllList.Items[selectedIndex + 1].Selected = true;
      this.saveSettings();
    }

    private void removeSelectedDll()
    {
      if (this.DllList.SelectedItems.Count == 1)
      {
        int selectedIndex = this.DllList.SelectedIndices[0];
        if (selectedIndex == 0)
          return;
        this.DllList.Items.RemoveAt(selectedIndex);
        if (this.DllList.Items.Count > selectedIndex && this.DllList.Items[selectedIndex] != null)
          this.DllList.Items[selectedIndex].Selected = true;
        else
          this.DllList.Items[selectedIndex - 1].Selected = true;
      }
      else
      {
        for (int index = this.DllList.Items.Count - 1; index > 0; --index)
        {
          if (this.DllList.Items[index].Selected)
            this.DllList.Items.RemoveAt(index);
        }
      }
    }

    private void AutoInjectCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (this.AutoInjectCheckBox.Checked || !this.AutoInjectTimer.Enabled)
        return;
      this.AutoInjectTimer.Stop();
      this.InfoText.Text = "You may inject now.";
    }

    private void GameClosedTimer_Tick(object sender, EventArgs e)
    {
      this.GameClosedTimer.Stop();
      this.can_auto_inject = true;
    }

    private void ChangelogBtn_Click(object sender, EventArgs e) => new Changelog().Show();

    private void ReInjectTimer_Tick(object sender, EventArgs e)
    {
      this.ReInjectTimer.Stop();
      this.EnableReInject();
    }

    private void EnableReInject()
    {
      this.InjectBtn.Enabled = true;
      this.ProcessScanTimer.Start();
    }

    private void StandFolderBtn_Click(object sender, EventArgs e) => Process.Start(this.stand_dir);

    private void UpdCheckBtn_Click(object sender, EventArgs e)
    {
      if (this.checkForUpdate(true) == 0)
      {
        int num = (int) this.showMessageBox("Everything up-to-date.");
      }
      this.processGtaPidUpdate(false);
    }

    private void DllList_DragOver(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
      else
        e.Effect = DragDropEffects.None;
    }

    private void DllList_DragDrop(object sender, DragEventArgs e)
    {
      foreach (string path in (string[]) e.Data.GetData(DataFormats.FileDrop))
        this.addDll(path);
    }

    private void DllList_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Delete)
        return;
      this.removeSelectedDll();
    }

    private void LaunchBtn_Click(object sender, EventArgs e)
    {
      switch (((DropDownEntry) this.LauncherType.SelectedItem).Id)
      {
        case 0:
          Process.Start("com.epicgames.launcher://apps/9d2d0eb64d5c44529cece33fe2a46482?action=launch&silent=true");
          break;
        case 1:
          object obj = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", (object) null);
          if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
          {
            Process.Start("steam://run/271590");
            break;
          }
          int num = (int) this.showMessageBox("Whoops, looks like Steam isn't installed. Try selecting a different launcher in the dropdown.", icon: MessageBoxIcon.Exclamation);
          break;
        case 2:
          try
          {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Rockstar Games\\Grand Theft Auto V"))
            {
              string str = (string) registryKey?.GetValue("InstallFolder");
              if (str == null)
                break;
              Process.Start(str + "\\PlayGTAV.exe");
              break;
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.ToString());
            break;
          }
      }
    }

    private void LauncherType_SelectedIndexChanged(object sender, EventArgs e) => this.LaunchBtn.Focus();

    private void toggleInjectOrLaunchBtn(bool gameRunning)
    {
      this.InjectBtn.Visible = gameRunning;
      this.LauncherType.Visible = this.LaunchBtn.Visible = !gameRunning;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Launchpad));
      this.InjectBtn = new Button();
      this.ProcessScanTimer = new System.Windows.Forms.Timer(this.components);
      this.InfoText = new Label();
      this.AutoInjectCheckBox = new CheckBox();
      this.CustomDllDialog = new OpenFileDialog();
      this.AdvancedBtn = new Button();
      this.RemoveBtn = new Button();
      this.UpBtn = new Button();
      this.DownBtn = new Button();
      this.AutoInjectDelaySeconds = new NumericUpDown();
      this.AutoInjectDelayLabel = new Label();
      this.AutoInjectTimer = new System.Windows.Forms.Timer(this.components);
      this.GameClosedTimer = new System.Windows.Forms.Timer(this.components);
      this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
      this.ChanglogBtn = new Button();
      this.ReInjectTimer = new System.Windows.Forms.Timer(this.components);
      this.StandFolderBtn = new Button();
      this.UpdCheckBtn = new Button();
      this.DllList = new ListView();
      this.Column = new ColumnHeader();
      this.LauncherType = new ComboBox();
      this.dropDownEntryBindingSource = new BindingSource(this.components);
      this.LaunchBtn = new Button();
      this.progressBar1 = new ProgressBar();
      this.AddBtn = new Button();
      this.AutoInjectDelaySeconds.BeginInit();
      ((ISupportInitialize) this.dropDownEntryBindingSource).BeginInit();
      this.SuspendLayout();
      this.InjectBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.InjectBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.InjectBtn.FlatStyle = FlatStyle.Flat;
      this.InjectBtn.Location = new Point(12, 12);
      this.InjectBtn.Name = "InjectBtn";
      this.InjectBtn.Size = new Size(208, 23);
      this.InjectBtn.TabIndex = 0;
      this.InjectBtn.Text = "Inject";
      this.InjectBtn.TextAlign = ContentAlignment.MiddleLeft;
      this.InjectBtn.UseVisualStyleBackColor = false;
      this.InjectBtn.Click += new EventHandler(this.InjectBtn_Click);
      this.ProcessScanTimer.Interval = 1000;
      this.ProcessScanTimer.Tick += new EventHandler(this.ProcessScanTimer_Tick);
      this.InfoText.AutoSize = true;
      this.InfoText.Location = new Point(9, 153);
      this.InfoText.Name = "InfoText";
      this.InfoText.Size = new Size(117, 13);
      this.InfoText.TabIndex = 9;
      this.InfoText.Text = "Checking for updates...";
      this.AutoInjectCheckBox.AutoSize = true;
      this.AutoInjectCheckBox.FlatStyle = FlatStyle.Flat;
      this.AutoInjectCheckBox.ForeColor = System.Drawing.Color.White;
      this.AutoInjectCheckBox.Location = new Point(12, 41);
      this.AutoInjectCheckBox.Name = "AutoInjectCheckBox";
      this.AutoInjectCheckBox.Size = new Size(202, 17);
      this.AutoInjectCheckBox.TabIndex = 4;
      this.AutoInjectCheckBox.Text = "Automatically inject when game starts.";
      this.AutoInjectCheckBox.UseVisualStyleBackColor = true;
      this.AutoInjectCheckBox.CheckedChanged += new EventHandler(this.AutoInjectCheckBox_CheckedChanged);
      this.CustomDllDialog.Filter = "DLL files|*.dll|All files|*.*";
      this.CustomDllDialog.FileOk += new CancelEventHandler(this.CustomDllDialog_FileOk);
      this.AdvancedBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.AdvancedBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.AdvancedBtn.FlatStyle = FlatStyle.Flat;
      this.AdvancedBtn.Location = new Point(12, 122);
      this.AdvancedBtn.Name = "AdvancedBtn";
      this.AdvancedBtn.Size = new Size(208, 23);
      this.AdvancedBtn.TabIndex = 8;
      this.AdvancedBtn.Text = "Advanced";
      this.AdvancedBtn.TextAlign = ContentAlignment.MiddleLeft;
      this.AdvancedBtn.UseVisualStyleBackColor = false;
      this.AdvancedBtn.Click += new EventHandler(this.AdvancedBtn_Click);
      this.RemoveBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.RemoveBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.RemoveBtn.FlatStyle = FlatStyle.Flat;
      this.RemoveBtn.Location = new Point(620, 12);
      this.RemoveBtn.Name = "RemoveBtn";
      this.RemoveBtn.Size = new Size(65, 23);
      this.RemoveBtn.TabIndex = 13;
      this.RemoveBtn.Text = "Remove";
      this.RemoveBtn.UseVisualStyleBackColor = false;
      this.RemoveBtn.Click += new EventHandler(this.RemoveBtn_Click);
      this.UpBtn.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.UpBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.UpBtn.FlatStyle = FlatStyle.Flat;
      this.UpBtn.Location = new Point(530, 12);
      this.UpBtn.Name = "UpBtn";
      this.UpBtn.Size = new Size(17, 23);
      this.UpBtn.TabIndex = 15;
      this.UpBtn.Text = "↑";
      this.UpBtn.UseVisualStyleBackColor = false;
      this.UpBtn.Click += new EventHandler(this.UpBtn_Click);
      this.DownBtn.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.DownBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.DownBtn.FlatStyle = FlatStyle.Flat;
      this.DownBtn.Location = new Point(547, 12);
      this.DownBtn.Name = "DownBtn";
      this.DownBtn.Size = new Size(17, 23);
      this.DownBtn.TabIndex = 16;
      this.DownBtn.Text = "↓";
      this.DownBtn.UseVisualStyleBackColor = false;
      this.DownBtn.Click += new EventHandler(this.DownBtn_Click);
      this.AutoInjectDelaySeconds.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.AutoInjectDelaySeconds.ForeColor = System.Drawing.Color.White;
      this.AutoInjectDelaySeconds.Location = new Point(415, 15);
      this.AutoInjectDelaySeconds.Maximum = new Decimal(new int[4]
      {
        60,
        0,
        0,
        0
      });
      this.AutoInjectDelaySeconds.Name = "AutoInjectDelaySeconds";
      this.AutoInjectDelaySeconds.Size = new Size(45, 20);
      this.AutoInjectDelaySeconds.TabIndex = 11;
      this.AutoInjectDelayLabel.AutoSize = true;
      this.AutoInjectDelayLabel.ForeColor = System.Drawing.Color.White;
      this.AutoInjectDelayLabel.Location = new Point(232, 17);
      this.AutoInjectDelayLabel.Name = "AutoInjectDelayLabel";
      this.AutoInjectDelayLabel.Size = new Size(181, 13);
      this.AutoInjectDelayLabel.TabIndex = 10;
      this.AutoInjectDelayLabel.Text = "Automatic Injection Delay (Seconds):";
      this.AutoInjectTimer.Interval = 1;
      this.AutoInjectTimer.Tick += new EventHandler(this.AutoInjectTimer_Tick);
      this.GameClosedTimer.Interval = 10000;
      this.GameClosedTimer.Tick += new EventHandler(this.GameClosedTimer_Tick);
      this.UpdateTimer.Interval = 1;
      this.UpdateTimer.Tick += new EventHandler(this.UpdateTimer_Tick);
      this.ChanglogBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.ChanglogBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.ChanglogBtn.FlatStyle = FlatStyle.Flat;
      this.ChanglogBtn.ForeColor = System.Drawing.Color.White;
      this.ChanglogBtn.Location = new Point(146, 64);
      this.ChanglogBtn.Name = "ChanglogBtn";
      this.ChanglogBtn.Size = new Size(74, 23);
      this.ChanglogBtn.TabIndex = 6;
      this.ChanglogBtn.Text = "Changelog";
      this.ChanglogBtn.TextAlign = ContentAlignment.MiddleLeft;
      this.ChanglogBtn.UseVisualStyleBackColor = false;
      this.ChanglogBtn.Click += new EventHandler(this.ChangelogBtn_Click);
      this.ReInjectTimer.Interval = 3000;
      this.ReInjectTimer.Tick += new EventHandler(this.ReInjectTimer_Tick);
      this.StandFolderBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.StandFolderBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.StandFolderBtn.FlatStyle = FlatStyle.Flat;
      this.StandFolderBtn.Location = new Point(12, 93);
      this.StandFolderBtn.Name = "StandFolderBtn";
      this.StandFolderBtn.Size = new Size(208, 23);
      this.StandFolderBtn.TabIndex = 7;
      this.StandFolderBtn.Text = "Open Stand Folder";
      this.StandFolderBtn.TextAlign = ContentAlignment.MiddleLeft;
      this.StandFolderBtn.UseVisualStyleBackColor = false;
      this.StandFolderBtn.Click += new EventHandler(this.StandFolderBtn_Click);
      this.UpdCheckBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.UpdCheckBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.UpdCheckBtn.FlatStyle = FlatStyle.Flat;
      this.UpdCheckBtn.ForeColor = System.Drawing.Color.White;
      this.UpdCheckBtn.Location = new Point(12, 64);
      this.UpdCheckBtn.Name = "UpdCheckBtn";
      this.UpdCheckBtn.Size = new Size(128, 23);
      this.UpdCheckBtn.TabIndex = 5;
      this.UpdCheckBtn.Text = "Check For Updates";
      this.UpdCheckBtn.TextAlign = ContentAlignment.MiddleLeft;
      this.UpdCheckBtn.UseVisualStyleBackColor = false;
      this.UpdCheckBtn.Click += new EventHandler(this.UpdCheckBtn_Click);
      this.DllList.AllowDrop = true;
      this.DllList.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.DllList.BorderStyle = BorderStyle.FixedSingle;
      this.DllList.CheckBoxes = true;
      this.DllList.Columns.AddRange(new ColumnHeader[1]
      {
        this.Column
      });
      this.DllList.ForeColor = SystemColors.Window;
      this.DllList.HideSelection = false;
      this.DllList.Location = new Point(235, 41);
      this.DllList.Margin = new Padding(6);
      this.DllList.Name = "DllList";
      this.DllList.Size = new Size(450, 122);
      this.DllList.TabIndex = 14;
      this.DllList.UseCompatibleStateImageBehavior = false;
      this.DllList.View = View.List;
      this.DllList.DragDrop += new DragEventHandler(this.DllList_DragDrop);
      this.DllList.DragOver += new DragEventHandler(this.DllList_DragOver);
      this.DllList.KeyUp += new KeyEventHandler(this.DllList_KeyUp);
      this.Column.Text = "";
      this.Column.Width = 450;
      this.LauncherType.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.LauncherType.DataSource = (object) this.dropDownEntryBindingSource;
      this.LauncherType.DisplayMember = "Name";
      this.LauncherType.DropDownStyle = ComboBoxStyle.DropDownList;
      this.LauncherType.FlatStyle = FlatStyle.Flat;
      this.LauncherType.ForeColor = SystemColors.Window;
      this.LauncherType.FormattingEnabled = true;
      this.LauncherType.ItemHeight = 13;
      this.LauncherType.Location = new Point(92, 14);
      this.LauncherType.Name = "LauncherType";
      this.LauncherType.Size = new Size(128, 21);
      this.LauncherType.TabIndex = 2;
      this.LauncherType.ValueMember = "Id";
      this.LauncherType.SelectedIndexChanged += new EventHandler(this.LauncherType_SelectedIndexChanged);
      this.dropDownEntryBindingSource.DataSource = (object) typeof (DropDownEntry);
      this.LaunchBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.LaunchBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.LaunchBtn.FlatStyle = FlatStyle.Flat;
      this.LaunchBtn.Location = new Point(12, 12);
      this.LaunchBtn.Name = "LaunchBtn";
      this.LaunchBtn.Size = new Size(74, 23);
      this.LaunchBtn.TabIndex = 1;
      this.LaunchBtn.Text = "Launch";
      this.LaunchBtn.UseVisualStyleBackColor = false;
      this.LaunchBtn.Click += new EventHandler(this.LaunchBtn_Click);
      this.progressBar1.Location = new Point(12, 12);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new Size(208, 23);
      this.progressBar1.TabIndex = 0;
      this.progressBar1.Visible = false;
      this.AddBtn.BackColor = System.Drawing.Color.FromArgb(37, 40, 43);
      this.AddBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(22, 25, 29);
      this.AddBtn.FlatStyle = FlatStyle.Flat;
      this.AddBtn.Location = new Point(567, 12);
      this.AddBtn.Name = "AddBtn";
      this.AddBtn.Size = new Size(47, 23);
      this.AddBtn.TabIndex = 17;
      this.AddBtn.Text = "Add";
      this.AddBtn.UseVisualStyleBackColor = false;
      this.AddBtn.Click += new EventHandler(this.AddBtn_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
      this.ClientSize = new Size(694, 175);
      this.Controls.Add((Control) this.AddBtn);
      this.Controls.Add((Control) this.progressBar1);
      this.Controls.Add((Control) this.LaunchBtn);
      this.Controls.Add((Control) this.LauncherType);
      this.Controls.Add((Control) this.DllList);
      this.Controls.Add((Control) this.UpdCheckBtn);
      this.Controls.Add((Control) this.StandFolderBtn);
      this.Controls.Add((Control) this.ChanglogBtn);
      this.Controls.Add((Control) this.AutoInjectDelayLabel);
      this.Controls.Add((Control) this.AutoInjectDelaySeconds);
      this.Controls.Add((Control) this.UpBtn);
      this.Controls.Add((Control) this.DownBtn);
      this.Controls.Add((Control) this.RemoveBtn);
      this.Controls.Add((Control) this.AdvancedBtn);
      this.Controls.Add((Control) this.AutoInjectCheckBox);
      this.Controls.Add((Control) this.InfoText);
      this.Controls.Add((Control) this.InjectBtn);
      this.ForeColor = System.Drawing.Color.White;
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.Name = nameof (Launchpad);
      this.Text = "Stand Launchpad";
      this.FormClosing += new FormClosingEventHandler(this.Launchpad_FormClosing);
      this.AutoInjectDelaySeconds.EndInit();
      ((ISupportInitialize) this.dropDownEntryBindingSource).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate IntPtr VirtualAllocExDelegate(
      IntPtr hProcess,
      IntPtr lpAddress,
      IntPtr dwSize,
      uint flAllocationType,
      uint flProtect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int WriteProcessMemoryDelegate(
      IntPtr hProcess,
      IntPtr lpBaseAddress,
      byte[] buffer,
      uint size,
      int lpNumberOfBytesWritten);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate IntPtr CreateRemoteThreadDelegate(
      IntPtr hProcess,
      IntPtr lpThreadAttribute,
      IntPtr dwStackSize,
      IntPtr lpStartAddress,
      IntPtr lpParameter,
      uint dwCreationFlags,
      IntPtr lpThreadId);
  }
}
