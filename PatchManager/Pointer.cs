// Decompiled with JetBrains decompiler
// Type: RoyalRevolt2.Pointer
// Assembly: RoyalRevolt2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F26EAA15-FE83-4F26-BCC4-D229A2C36673
// Assembly location: C:\Users\Daniele\Downloads\RoyalRevolt2.exe

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PatchManager
{
  public class Pointer : IDisposable
  {
    private Process process;
    private IntPtr processHandle;
    private bool isDisposed;
    public const string OffsetPattern = "(\\+|\\-){0,1}(0x){0,1}[a-fA-F0-9]{1,}";

    public Process Process
    {
      get
      {
        return this.process;
      }
    }

    public Pointer(Process process)
    {
      if (process == null)
        throw new ArgumentNullException("process");
      this.process = process;
      this.processHandle = Pointer.OpenProcess(Pointer.ProcessAccessType.PROCESS_VM_OPERATION | Pointer.ProcessAccessType.PROCESS_VM_READ | Pointer.ProcessAccessType.PROCESS_VM_WRITE, true, (uint) process.Id);
      if (this.processHandle == IntPtr.Zero)
        throw new InvalidOperationException("Could not open the process");
    }

    ~Pointer()
    {
      this.Dispose(false);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint written);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint read);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess([MarshalAs(UnmanagedType.U4)] Pointer.ProcessAccessType access, [MarshalAs(UnmanagedType.Bool)] bool inheritHandler, uint processId);

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr objectHandle);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this.isDisposed)
        return;
      Pointer.CloseHandle(this.processHandle);
      this.process = (Process) null;
      this.processHandle = IntPtr.Zero;
      this.isDisposed = true;
    }

    protected ProcessModule FindModule(string name)
    {
      try
      {
        if (string.IsNullOrEmpty(name))
          throw new ArgumentNullException("name");
        foreach (ProcessModule module in (ReadOnlyCollectionBase) this.process.Modules)
        {
          if (module.ModuleName.ToLower() == name.ToLower())
            return module;
        }
      }
      catch
      {
      }
      return (ProcessModule) null;
    }

    public IntPtr GetAddress(string moduleName, IntPtr baseAddress, int[] offsets)
    {
      if (string.IsNullOrEmpty(moduleName))
        throw new ArgumentNullException("moduleName");
      ProcessModule module = this.FindModule(moduleName);
      if (module == null)
        return IntPtr.Zero;
      return this.GetAddress((IntPtr) (module.BaseAddress.ToInt32() + baseAddress.ToInt32()), offsets);
    }

    public IntPtr GetAddress(IntPtr baseAddress, int[] offsets)
    {
      if (baseAddress == IntPtr.Zero)
        throw new ArgumentException("Invalid base address");
      int num = baseAddress.ToInt32();
      if (offsets != null && offsets.Length != 0)
      {
        byte[] numArray = new byte[4];
        foreach (int offset in offsets)
          num = this.ReadInt32((IntPtr) num) + offset;
      }
      return (IntPtr) num;
    }

    public IntPtr GetAddress(string address)
    {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException("address");
      string moduleName = (string) null;
      int num1 = address.IndexOf('"');
      if (num1 != -1)
      {
        int num2 = address.IndexOf('"', num1 + 1);
        if (num2 == -1)
          throw new ArgumentException("Invalid module name. Could not find matching \"");
        moduleName = address.Substring(num1 + 1, num2 - 1);
        address = address.Substring(num2 + 1);
      }
      int[] addressOffsets = Pointer.GetAddressOffsets(address);
      int[] offsets = (int[]) null;
      IntPtr baseAddress = addressOffsets == null || addressOffsets.Length == 0 ? IntPtr.Zero : (IntPtr) addressOffsets[0];
      if (addressOffsets != null && addressOffsets.Length > 1)
      {
        offsets = new int[addressOffsets.Length - 1];
        for (int index = 0; index < addressOffsets.Length - 1; ++index)
          offsets[index] = addressOffsets[index + 1];
      }
      if (moduleName != null)
        return this.GetAddress(moduleName, baseAddress, offsets);
      return this.GetAddress(baseAddress, offsets);
    }

    protected static int[] GetAddressOffsets(string address)
    {
      if (string.IsNullOrEmpty(address))
        return new int[0];
      MatchCollection matchCollection = Regex.Matches(address, "(\\+|\\-){0,1}(0x){0,1}[a-fA-F0-9]{1,}");
      int[] numArray = new int[matchCollection.Count];
      for (int index = 0; index < matchCollection.Count; ++index)
      {
        char ch = matchCollection[index].Value[0];
        string str;
        switch (ch)
        {
          case '+':
          case '-':
            str = matchCollection[index].Value.Substring(1);
            break;
          default:
            str = matchCollection[index].Value;
            break;
        }
        numArray[index] = Convert.ToInt32(str, 16);
        if ((int) ch == 45)
          numArray[index] = -numArray[index];
      }
      return numArray;
    }

    public void ReadMemory(IntPtr address, byte[] buffer, int size)
    {
      if (this.isDisposed)
        throw new ObjectDisposedException("Memory");
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (size <= 0)
        throw new ArgumentException("Size must be greater than zero");
      if (address == IntPtr.Zero)
        throw new ArgumentException("Invalid address");
      uint read = 0;
      if (!Pointer.ReadProcessMemory(this.processHandle, address, buffer, (uint) size, ref read) || (long) read != (long) size)
        throw new AccessViolationException();
    }

    public void WriteMemory(IntPtr address, byte[] buffer, int size)
    {
      if (this.isDisposed)
        throw new ObjectDisposedException("Memory");
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (size <= 0)
        throw new ArgumentException("Size must be greater than zero");
      if (address == IntPtr.Zero)
        throw new ArgumentException("Invalid address");
      uint written = 0;
      if (!Pointer.WriteProcessMemory(this.processHandle, address, buffer, (uint) size, ref written) || (long) written != (long) size)
        throw new AccessViolationException();
    }

    public int ReadInt32(IntPtr address)
    {
      byte[] buffer = new byte[4];
      this.ReadMemory(address, buffer, 4);
      return BitConverter.ToInt32(buffer, 0);
    }

    public uint ReadUInt32(IntPtr address)
    {
      byte[] buffer = new byte[4];
      this.ReadMemory(address, buffer, 4);
      return BitConverter.ToUInt32(buffer, 0);
    }

    public float ReadFloat(IntPtr address)
    {
      byte[] buffer = new byte[4];
      this.ReadMemory(address, buffer, 4);
      return BitConverter.ToSingle(buffer, 0);
    }

    public double ReadDouble(IntPtr address)
    {
      byte[] buffer = new byte[8];
      this.ReadMemory(address, buffer, 8);
      return BitConverter.ToDouble(buffer, 0);
    }

    public void WriteUInt32(IntPtr address, uint value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      this.WriteMemory(address, bytes, 4);
    }

    public void WriteInt32(IntPtr address, int value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      this.WriteMemory(address, bytes, 4);
    }

    public void WriteFloat(IntPtr address, float value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      this.WriteMemory(address, bytes, 4);
    }

    public void WriteDouble(IntPtr address, double value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      this.WriteMemory(address, bytes, 8);
    }

    [Flags]
    public enum ProcessAccessType
    {
      PROCESS_TERMINATE = 1,
      PROCESS_CREATE_THREAD = 2,
      PROCESS_SET_SESSIONID = 4,
      PROCESS_VM_OPERATION = 8,
      PROCESS_VM_READ = 16,
      PROCESS_VM_WRITE = 32,
      PROCESS_DUP_HANDLE = 64,
      PROCESS_CREATE_PROCESS = 128,
      PROCESS_SET_QUOTA = 256,
      PROCESS_SET_INFORMATION = 512,
      PROCESS_QUERY_INFORMATION = 1024,
    }
  }
}
