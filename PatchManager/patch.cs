// Decompiled with JetBrains decompiler
// Type: RoyalRevolt2.patch
// Assembly: RoyalRevolt2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F26EAA15-FE83-4F26-BCC4-D229A2C36673
// Assembly location: C:\Users\Daniele\Downloads\RoyalRevolt2.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace PatchManager
{
  internal class patch
  {
    private List<patchItem> items;
    private IntPtr processHandle;
    private IntPtr patchAddr;
    private byte[] findByte;
    private byte[] replaceByte;
    private uint pid;

    private static List<patch.MEMORY_BASIC_INFORMATION> MemReg { get; set; }

    public List<patchItem> Items
    {
      get
      {
        return this.items;
      }
      set
      {
        this.items = value;
      }
    }

    public IntPtr ProcessHandle
    {
      get
      {
        return this.processHandle;
      }
      set
      {
        this.processHandle = value;
      }
    }

    public IntPtr PatchAddr
    {
      get
      {
        return this.patchAddr;
      }
      set
      {
        this.patchAddr = value;
      }
    }

    public byte[] FindByte
    {
      get
      {
        return this.findByte;
      }
      set
      {
        this.findByte = value;
      }
    }

    public byte[] ReplaceByte
    {
      get
      {
        return this.replaceByte;
      }
      set
      {
        this.replaceByte = value;
      }
    }

    public uint Pid
    {
      get
      {
        return this.pid;
      }
      set
      {
        this.pid = value;
      }
    }

    public patch()
    {
      this.patchAddr = IntPtr.Zero;
      this.items = new List<patchItem>();
    }

    public patch(uint Pid)
    {
      this.patchAddr = IntPtr.Zero;
      this.items = new List<patchItem>();
      this.pid = Pid;
    }

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint size, int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out patch.MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    private void MemInfo(IntPtr pHandle)
    {
      IntPtr lpAddress = new IntPtr();
      while (true)
      {
        patch.MEMORY_BASIC_INFORMATION lpBuffer = new patch.MEMORY_BASIC_INFORMATION();
        if (patch.VirtualQueryEx(pHandle, lpAddress, out lpBuffer, Marshal.SizeOf((object) lpBuffer)) != 0)
        {
          if (((int) lpBuffer.State & 4096) != 0 && ((int) lpBuffer.Protect & 256) == 0)
            patch.MemReg.Add(lpBuffer);
          lpAddress = new IntPtr(lpBuffer.BaseAddress.ToInt32() + lpBuffer.RegionSize.ToInt32());
        }
        else
          break;
      }
    }

    private IntPtr _Scan(byte[] sIn, byte[] sFor)
    {
      int[] numArray = new int[256];
      int num1 = 0;
      int num2 = sFor.Length - 1;
      for (int index = 0; index < 256; ++index)
        numArray[index] = sFor.Length;
      for (int index = 0; index < num2; ++index)
        numArray[(int) sFor[index]] = num2 - index;
      while (num1 <= sIn.Length - sFor.Length)
      {
        for (int index = num2; (int) sIn[num1 + index] == (int) sFor[index]; --index)
        {
          if (index == 0)
            return new IntPtr(num1);
        }
        num1 += numArray[(int) sIn[num1 + num2]];
      }
      return IntPtr.Zero;
    }

    private void patchScan()
    {
      patch.MemReg = new List<patch.MEMORY_BASIC_INFORMATION>();
      this.MemInfo(this.processHandle);
      for (int index = 0; index < this.items.Count; ++index)
        this.items[index].ScanCheck = false;
      for (int index1 = 0; index1 < patch.MemReg.Count; ++index1)
      {
        byte[] numArray = new byte[patch.MemReg[index1].RegionSize.ToInt32()];
        patch.ReadProcessMemory(this.processHandle, patch.MemReg[index1].BaseAddress, numArray, (uint) patch.MemReg[index1].RegionSize.ToInt32(), 0);
        for (int index2 = 0; index2 < this.items.Count; ++index2)
        {
          if ((!this.items[index2].Enabled || !this.items[index2].VerifyCheck) && !this.items[index2].ScanCheck)
          {
            IntPtr num = this._Scan(numArray, this.items[index2].FindByte);
            if (num != IntPtr.Zero)
            {
              this.items[index2].ScanCheck = true;
              this.items[index2].VerifyCheck = true;
              this.items[index2].PatchAddr = new IntPtr(patch.MemReg[index1].BaseAddress.ToInt32() + num.ToInt32());
            }
          }
        }
      }
    }

    private void patchReplace()
    {
      for (int index = 0; index < this.items.Count; ++index)
      {
        if (this.items[index].Enabled && this.items[index].ScanCheck && this.items[index].VerifyCheck)
        {
          int lpNumberOfBytesWritten = 0;
          patch.WriteProcessMemory(this.processHandle, this.items[index].PatchAddr, this.items[index].ReplaceByte, (uint) this.items[index].ReplaceByte.Length, lpNumberOfBytesWritten);
        }
      }
    }

    private void verifyPatch()
    {
      for (int index = 0; index < this.items.Count; ++index)
      {
        if (this.items[index].Enabled)
        {
          this.items[index].VerifyCheck = false;
          if (this.items[index].PatchAddr != IntPtr.Zero)
          {
            byte[] first = new byte[this.items[index].ReplaceByte.Length];
            IntPtr processHandle = this.processHandle;
            IntPtr patchAddr = this.items[index].PatchAddr;
            byte[] buffer = first;
            int length = buffer.Length;
            int lpNumberOfBytesRead = 0;
            if (patch.ReadProcessMemory(processHandle, patchAddr, buffer, (uint) length, lpNumberOfBytesRead) && patch.compareArray(first, this.items[index].ReplaceByte))
            {
              this.items[index].VerifyCheck = true;
              this.items[index].ScanCheck = false;
            }
          }
        }
      }
    }

    public static bool compareArray(byte[] first, byte[] second)
    {
      if (first.Length > second.Length)
        return false;
      for (int index = 0; index < first.Length; ++index)
      {
        if ((int) first[index] != (int) second[index])
          return false;
      }
      return true;
    }

    public static byte[] string2Byte(string buffer)
    {
      string[] strArray = buffer.Split(' ');
      byte[] numArray = new byte[strArray.Length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = byte.Parse(strArray[index], NumberStyles.HexNumber);
      return numArray;
    }

    public void patchMemory()
    {
      this.ProcessHandle = patch.OpenProcess(2035711U, false, this.pid);
      for (int index = 0; index < this.items.Count; ++index)
      {
        if (this.items[index].Enabled)
          this.items[index].Status = patchItem.patchStatus.In_Progress;
      }
      this.verifyPatch();
      this.patchScan();
      this.patchReplace();
      for (int index = 0; index < this.items.Count; ++index)
      {
        if (this.items[index].Enabled && this.items[index].VerifyCheck && this.Items[index].ScanCheck)
          this.items[index].Status = patchItem.patchStatus.Patched;
        if (this.items[index].Enabled && this.items[index].VerifyCheck && !this.Items[index].ScanCheck)
          this.items[index].Status = patchItem.patchStatus.Already_Patched;
        if (this.items[index].Enabled && !this.items[index].VerifyCheck && !this.Items[index].ScanCheck)
          this.items[index].Status = patchItem.patchStatus.Signature_Not_Found;
      }
      patch.CloseHandle(this.ProcessHandle);
    }

    protected struct MEMORY_BASIC_INFORMATION
    {
      public IntPtr BaseAddress;
      public IntPtr AllocationBase;
      public uint AllocationProtect;
      public IntPtr RegionSize;
      public uint State;
      public uint Protect;
      public uint Type;
    }
  }
}
