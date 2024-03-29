///////////////////////////////////////////////////////////////////////////////
//InteropBits.cs
//COM Interop C# classes for accessing BITS API.
//
//Copyright (c)  Jason Clark  
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

namespace Bits {
   [GuidAttribute("4991D34B-80A1-4291-83B6-3328366B9097")]
   [ClassInterfaceAttribute(ClassInterfaceType.None)]
   [ComImportAttribute()]
   class BackgroundCopyManager{}

   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [GuidAttribute("5CE34C0D-0DC9-4C1F-897C-DAA1B78CEE7C")]
   [ComImportAttribute()]
   interface IBackgroundCopyManager {
      void CreateJob([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, BG_JOB_TYPE Type, out Guid pJobId, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob ppJob);
      void GetJob(ref Guid jobID, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob ppJob);
      void EnumJobs(uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyJobs ppenum);
      void GetErrorDescription([MarshalAs(UnmanagedType.Error)] int hResult, uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pErrorDescription);
   }

   [GuidAttribute("97EA99C7-0186-4AD4-8DF9-C5B4E0ED6B22")]
   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [ComImportAttribute()]
   interface IBackgroundCopyCallback {
      void JobTransferred([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob);
      void JobError([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, [MarshalAs(UnmanagedType.Interface)] IBackgroundCopyError pError);
      void JobModification([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, uint dwReserved);
   }

   [GuidAttribute("19C613A0-FCB8-4F28-81AE-897C3D078F81")]
   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [ComImportAttribute()]
   interface IBackgroundCopyError {
      void GetError(out BG_ERROR_CONTEXT pContext, [MarshalAs(UnmanagedType.Error)] out int pCode);
      void GetFile([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile pVal);
      void GetErrorDescription(uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pErrorDescription);
      void GetErrorContextDescription(uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pContextDescription);
      void GetProtocol([MarshalAs(UnmanagedType.LPWStr)] out string pProtocol);
   }

   [GuidAttribute("01B7BD23-FB88-4A77-8490-5891D3E4653A")]
   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [ComImportAttribute()]
   interface IBackgroundCopyFile {
      void GetRemoteName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
      void GetLocalName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
      void GetProgress(out _BG_FILE_PROGRESS pVal);
   }

   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [GuidAttribute("37668D37-507E-4160-9316-26306D150B12")]
   [ComImportAttribute()]
   interface IBackgroundCopyJob {
      void AddFileSet(uint cFileCount, ref _BG_FILE_INFO pFileSet);
      void AddFile([MarshalAs(UnmanagedType.LPWStr)] string RemoteUrl, [MarshalAs(UnmanagedType.LPWStr)] string LocalName);
      void EnumFiles([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles pEnum);
      void Suspend();
      void Resume();
      void Cancel();
      void Complete();
      void GetId(out Guid pVal);
      void GetType(out BG_JOB_TYPE pVal);
      void GetProgress(out _BG_JOB_PROGRESS pVal);
      void GetTimes(out _BG_JOB_TIMES pVal);
      void GetState(out BG_JOB_STATE pVal);
      void GetError([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyError ppError);
      void GetOwner([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
      void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Val);
      void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
      void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string Val);
      void GetDescription([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
      void SetPriority(BG_JOB_PRIORITY Val);
      void GetPriority(out BG_JOB_PRIORITY pVal);
      void SetNotifyFlags(uint Val);
      void GetNotifyFlags(out uint pVal);
      void SetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] object Val);
      void GetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] out object pVal);
      void SetMinimumRetryDelay(uint Seconds);
      void GetMinimumRetryDelay(out uint Seconds);
      void SetNoProgressTimeout(uint Seconds);
      void GetNoProgressTimeout(out uint Seconds);
      void GetErrorCount(out uint Errors);
      void SetProxySettings(BG_JOB_PROXY_USAGE ProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] string ProxyList, [MarshalAs(UnmanagedType.LPWStr)] string ProxyBypassList);
      void GetProxySettings(out BG_JOB_PROXY_USAGE pProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyList, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyBypassList);
      void TakeOwnership();
   }

   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [GuidAttribute("CA51E165-C365-424C-8D41-24AAA4FF3C40")]
   [ComImportAttribute()]
   interface IEnumBackgroundCopyFiles {
      void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile rgelt, out uint pceltFetched);
      void Skip(uint celt);
      void Reset();
      void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles ppenum);
      void GetCount(out uint puCount);
   }

   [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
   [GuidAttribute("1AF4F612-3B71-466F-8F58-7B6F73AC57AD")]
   [ComImportAttribute()]
   interface IEnumBackgroundCopyJobs {
      void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob rgelt, out uint pceltFetched);
      void Skip(uint celt);
      void Reset();
      void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyJobs ppenum);
      void GetCount(out uint puCount);
   }

   enum BG_ERROR_CONTEXT {
      BG_ERROR_CONTEXT_NONE = 0,
      BG_ERROR_CONTEXT_UNKNOWN = 1,
      BG_ERROR_CONTEXT_GENERAL_QUEUE_MANAGER = 2,
      BG_ERROR_CONTEXT_QUEUE_MANAGER_NOTIFICATION = 3,
      BG_ERROR_CONTEXT_LOCAL_FILE = 4,
      BG_ERROR_CONTEXT_REMOTE_FILE = 5,
      BG_ERROR_CONTEXT_GENERAL_TRANSPORT = 6,
   }

   enum BG_JOB_PRIORITY {
      BG_JOB_PRIORITY_FOREGROUND = 0,
      BG_JOB_PRIORITY_HIGH = 1,
      BG_JOB_PRIORITY_NORMAL = 2,
      BG_JOB_PRIORITY_LOW = 3,
   }

   enum BG_JOB_PROXY_USAGE {
      BG_JOB_PROXY_USAGE_PRECONFIG = 0,
      BG_JOB_PROXY_USAGE_NO_PROXY = 1,
      BG_JOB_PROXY_USAGE_OVERRIDE = 2,
   }

   enum BG_JOB_STATE {
      BG_JOB_STATE_QUEUED = 0,
      BG_JOB_STATE_CONNECTING = 1,
      BG_JOB_STATE_TRANSFERRING = 2,
      BG_JOB_STATE_SUSPENDED = 3,
      BG_JOB_STATE_ERROR = 4,
      BG_JOB_STATE_TRANSIENT_ERROR = 5,
      BG_JOB_STATE_TRANSFERRED = 6,
      BG_JOB_STATE_ACKNOWLEDGED = 7,
      BG_JOB_STATE_CANCELLED = 8,
   }

   enum BG_JOB_TYPE {
      BG_JOB_TYPE_DOWNLOAD = 0,
   }

   [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
   struct _BG_FILE_INFO {
      [MarshalAs(UnmanagedType.LPWStr)]
      public string RemoteName;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string LocalName;
   }

   [StructLayoutAttribute(LayoutKind.Sequential, Pack=8, Size=0)]
   struct _BG_FILE_PROGRESS {
      public ulong BytesTotal;
      public ulong BytesTransferred;
      public int Completed;
   }

   [StructLayoutAttribute(LayoutKind.Sequential, Pack=8, Size=0)]
   struct _BG_JOB_PROGRESS {
      public ulong BytesTotal;
      public ulong BytesTransferred;
      public uint FilesTotal;
      public uint FilesTransferred;
   }

   [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
   struct _BG_JOB_TIMES {
      public _FILETIME CreationTime;
      public _FILETIME ModificationTime;
      public _FILETIME TransferCompletionTime;
   }

   [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
   struct _FILETIME {
      public uint dwLowDateTime;
      public uint dwHighDateTime;
   }   
}
