// Decompiled with JetBrains decompiler
// Type: RoyalRevolt2.patchItem
// Assembly: RoyalRevolt2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F26EAA15-FE83-4F26-BCC4-D229A2C36673
// Assembly location: C:\Users\Daniele\Downloads\RoyalRevolt2.exe

using System;

namespace PatchManager
{
  public class patchItem
  {
    private IntPtr patchAddr = IntPtr.Zero;
    private bool scanCheck;
    private bool verifyCheck;
    private bool enabled;
    private byte[] findByte;
    private byte[] replaceByte;
    private patchItem.patchStatus status;

    public bool ScanCheck
    {
      get
      {
        return this.scanCheck;
      }
      set
      {
        this.scanCheck = value;
      }
    }

    public bool VerifyCheck
    {
      get
      {
        return this.verifyCheck;
      }
      set
      {
        this.verifyCheck = value;
      }
    }

    public bool Enabled
    {
      get
      {
        return this.enabled;
      }
      set
      {
        this.status = patchItem.patchStatus.Idle;
        if (!this.enabled)
          this.status = patchItem.patchStatus.Disabled;
        this.enabled = value;
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

    public patchItem.patchStatus Status
    {
      get
      {
        return this.status;
      }
      set
      {
        this.status = value;
      }
    }

    public patchItem()
    {
      this.enabled = true;
    }

    public patchItem(bool isEnabled)
    {
      this.enabled = isEnabled;
    }

    public string getStatusString()
    {
      string str = "";
      if (this.status == patchItem.patchStatus.Already_Patched)
        str = "Already patched";
      if (this.status == patchItem.patchStatus.In_Progress)
        str = "In progress";
      if (this.status == patchItem.patchStatus.Patched)
        str = "Patched";
      if (this.status == patchItem.patchStatus.Signature_Not_Found)
        str = "Signature not found";
      if (this.status == patchItem.patchStatus.Restored)
        str = "Restored";
      if (this.status == patchItem.patchStatus.Disabled)
        str = "Disabled";
      if (this.status == patchItem.patchStatus.Idle)
        str = "Idle";
      return str;
    }

    public enum patchStatus
    {
      Idle,
      Patched,
      Already_Patched,
      Signature_Not_Found,
      In_Progress,
      Restored,
      Disabled,
    }
  }
}
