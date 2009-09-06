///////////////////////////////////////////////////////////////////////////////
//Updater.cs
//Simple class to help an application automatically update itself over the 
//the internet.
//
//Copyright (c)  Jason Clark  
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using Bits;

/// <summary>
/// UpdateStatus: Enumerated type used to indicate the status of updates.  
/// ApplyUpdate means that an update is available to be applied
/// </summary>
public enum UpdateStatus{
   ContinueExecution,
   ApplyUpdate
}

/// <summary>
/// AutoUpdater: The type that exposes three public static methods for auto 
/// updating an application.
///    UpdateStatus GetUpdateStatus();
///    void ApplyUpdate();
///    void RelaunchExe(Boolean inProc);
/// </summary>
public class AutoUpdater{
   private AutoUpdater(){}
   static AutoUpdater(){      
      // Get AppBase directory
      baseDir = AppDomain.CurrentDomain.BaseDirectory;            
      try{
         // Create object to manage XML file
         xml = new XmlState(baseDir+"UpdateState.xml");         
         patchName = String.Format("{0}{1}.dll", xml.PatchFilename, xml.NextUpdate);
      }catch(IOException){}
   }

   // Handle polling and download
   public static UpdateStatus GetUpdateStatus(Boolean async){
      ThreadPool.QueueUserWorkItem(new WaitCallback(CleanupTmps)); // cleanup temporary files

      // If this is the first run of an exe, do update work
      if(xml != null && PatchExists(async)){
         return UpdateStatus.ApplyUpdate;
      }      
      
      return UpdateStatus.ContinueExecution;      
   }

   // Extract update files and copy into AppBase directory
   public static void ApplyUpdate(){
      Stream temp = null;
      try{
         // Can't apply an update that has not already downloaded
         if(!PatchExists(false)){
            throw new InvalidOperationException("Can't apply a non-existent update.");
         }

         // Load the update
         String name = Path.GetFileNameWithoutExtension(patchName);
         name = String.Format("{0}, PublicKeyToken={1}", name, xml.Originator);
         Assembly update = Assembly.Load(name);

         // Check to see if there is a type in the assembly with a custom
         // extraction routine
         Boolean customCall = false;
         Type[] types = update.GetTypes();
         foreach(Type t in types){
            MethodInfo[] methods = t.GetMethods(BindingFlags.Public|BindingFlags.Static);
            foreach(MethodInfo m in methods){
               if(m.GetParameters().Length == 0 && m.ReturnType == typeof(void)){
                  // Found one, lets call it
                  customCall = true;
                  m.Invoke(null, null);
               }
            }
         }
         
         // Apply the update if a custom call didn't happen
         if(!customCall) RenameAndExtractFiles(update);

         // Add one to the update number for poll
         xml.NextUpdate = xml.NextUpdate+1;
         
      }catch(FileLoadException e){
         HandleBrokenUpdate(e);         
      }catch(FileNotFoundException e){
         HandleBrokenUpdate(e);         
      }finally{
         if(temp != null)
            temp.Close();
      }
   }

   // Re-run the application either in another AppDomain or in another process
   public static void RelaunchExe(Boolean inProc){
      // Get assembly that started it all
      Assembly entry = Assembly.GetEntryAssembly();
      // "Intuit" its exe name
      String name = entry.GetName().Name+".exe";  
      if(inProc){
         // Create an AppDomain that shadow copies its files
         AppDomain current = AppDomain.CurrentDomain;
         AppDomainSetup info = current.SetupInformation;
         info.ShadowCopyFiles = true.ToString();         
         AppDomain domain = AppDomain.CreateDomain(GetTempName(), current.Evidence, info);
         
         // Relaunch the application in the new application domain
         // using the same command line arguments
         String[] argsOld = Environment.GetCommandLineArgs();
         String[] argsNew = new String[argsOld.Length-1];
         Array.Copy(argsOld, 1, argsNew, 0, argsNew.Length);
         domain.ExecuteAssembly(name, entry.Evidence, argsNew);
      }else{
         // Create a new process and relaunch the application
         Process.Start(name, Environment.CommandLine);
      }
   }
   
   static void HandleBrokenUpdate(Exception e){
      // Something was wrong with the update file
      // rename it so that it will attempt another download
      MakeUpdateTmp();
      throw new SecurityException("Assembly didn't check-out.  Bad ju-ju.", e); 
   }

   static void RenameAndExtractFiles(Assembly update){      
      // Get resource names from update assembly
      String[] resources = update.GetManifestResourceNames();
      Hashtable renameLog = new Hashtable();
      try{
         foreach(String s in resources){
            // If a current file exists with the same name, rename it
            if(File.Exists(s)){
               String tempName = GetTempName();
               File.Move(s, tempName);
               renameLog[tempName] = s;
            }
            // Copy the resource out into the new file
            // this does not take into consideration file dates and other similar
            // attributes (but probobly should).
            using(Stream res = update.GetManifestResourceStream(s), file = new FileStream(s, FileMode.CreateNew)){
               Int32 pseudoByte;
               while((pseudoByte = res.ReadByte())!=-1){
                  file.WriteByte((Byte)pseudoByte);
               }
            }
         }  
         // If we made it this far, it is safe to rename the update assembly
         MakeUpdateTmp();
      }catch{
         // Unwind failed operation
         foreach(DictionaryEntry d in renameLog){
            String filename = d.Value as String;
            if(File.Exists(filename)){
               File.Delete(filename);
            }
            File.Move(d.Key as String, filename);
         }
         throw; // rethrow whatever went wrong
      }
   }

   static void MakeUpdateTmp(){
      File.Move(patchName, GetTempName());
   }

   static void CleanupTmps(Object o){
      // Delete any file with the extension ".update_tempfile"
      String[] files = Directory.GetFiles(baseDir, "*.update_tempfile");
      foreach(String s in files){
         try{
            File.Delete(s);
         }catch(IOException){ // ignore some expected errors
         }catch(UnauthorizedAccessException){
         }catch(SecurityException){}
      }
   }

   // Generate a unique name using a Guid
   static String GetTempName(){
      return Guid.NewGuid().ToString()+".update_tempfile";
   }

   // Do the file download and polling logic
   static void HandleBits(Object o){
      IBackgroundCopyManager bcm = null;
      IBackgroundCopyJob job=null;
      try{
         // Create BITS object
         bcm = (IBackgroundCopyManager)new BackgroundCopyManager();         
         Guid jobID=Guid.Empty;         
         if(xml.BitsJob != Guid.Empty){ // Do we already have a job in place?
            jobID = xml.BitsJob;
            BG_JOB_STATE state;
            try{
               bcm.GetJob(ref jobID, out job); // Get the BITS job object
               job.GetState(out state);        // check its state
               switch(state){
               case BG_JOB_STATE.BG_JOB_STATE_ERROR: // If it is an error, re-poll
                  job.Complete();
                  xml.BitsJob = Guid.Empty;
                  Marshal.ReleaseComObject(job);
                  job = null;
                  break;
               case BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED: // If we got the job
                  job.Complete();                          // then complete it
                  xml.BitsJob = Guid.Empty;
                  return;
               default:
                  return;
               }
            }catch(COMException e){               
               if(e.ErrorCode == unchecked((Int32)0x80200001)){ // Job doesn't exist
                  xml.BitsJob = Guid.Empty;
               }else
                  throw; // Some other error we didn't expect
            }catch(UnauthorizedAccessException){ 
               return; // We don't have access to the current job... no biggie
            }                                  
         }                  
         
         // Create a bits job to download the next expected update
         bcm.CreateJob("Application Update", 
            BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out jobID, out job);
         job.SetDescription("Updating application at "+baseDir);
         String resource;
         if(xml.PatchUrl[xml.PatchUrl.Length-1] == '/'){
            resource = xml.PatchUrl+patchName;
         }else{
            resource = 
               String.Format("{0}/{1}", xml.PatchUrl, patchName);
         }
         // Add the file to the job
         job.AddFile(resource, baseDir+patchName);
         job.Resume(); // start the job in action
         xml.BitsJob = jobID; // save the Job Guid
      }finally{         
         // cleanup
         if(job != null) 
            Marshal.ReleaseComObject(job);
         if(bcm != null)
            Marshal.ReleaseComObject(bcm);         
      }      
   }

   // Test to see if the next patch is already in the file system
   static Boolean PatchExists(Boolean async){ 
      if(File.Exists(patchName))
         return true;
      if(async){
         ThreadPool.QueueUserWorkItem(new WaitCallback(HandleBits));
         return false;
      }else{
         HandleBits(null);
         return File.Exists(patchName);
      }      
   }

   static String patchName = null;   
   static XmlState xml=null;
   static String baseDir=null;       
}

/// <summary>
/// XmlState: This object abstracts an XML file used for update status and 
/// configuration information.  This could be easily extended to abstract
/// the Job guid into isolated storage, so that updates are managed on a per
/// user basis.
/// </summary>
class XmlState{
   // .Ctor for creating the object and perhaps the XML file itself
   public XmlState(String path){
      xmlFileName = path;
      try{
         OpenXml();           
      }catch(FileNotFoundException){         
         CreateStatusStream();         
      }

      try{job = new Guid(xml.bitsJob);
      }catch(FormatException){job = Guid.Empty;}
   }

   // Properties for accessing information in the XML file
   public String Originator{get{return xml.originator;}}
   public String PatchFilename{get{return xml.patch.patchFilename;}}
   public String PatchUrl{get{return xml.patch.patchUrl;}}   
   public String Filename{get{return xmlFileName;}}

   // Writable properties
   public Int32 NextUpdate{
      get{
         return xml.nextUpdate;
      }
      set{
         xml.nextUpdate = value;
         FlushXml();
      }
   }

   private Guid job;
   public Guid BitsJob{      
      get{return job;}
      set{
         job = value;
         xml.bitsJob = job.ToString();
         FlushXml();
      }
   }

   private XmlSerializer ser = new XmlSerializer(typeof(XmlStatus));
   private XmlStatus xml = null;
   private Stream xmlFile = null;
   static String xmlFileName=null;

   void OpenXml(){
      xmlFile = new FileStream(xmlFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
      xml = (XmlStatus)ser.Deserialize(xmlFile);      
   }

   // If the XML file does not exist it must be created
   void CreateDefaultXml(){
      xml = new XmlStatus();
      xml.nextUpdate = 1;
      xml.patch.patchFilename = "Update";
      // Must be changed to reflect the update URL for the app
      xml.patch.patchUrl = "http://localhost/updates"; 
      xml.bitsJob = String.Empty;
      
      Assembly exe = Assembly.GetEntryAssembly();      
      Byte[] bytes = exe.GetName().GetPublicKeyToken();
      String token = BitConverter.ToString(bytes).Replace("-", String.Empty);      
      if(token==String.Empty)
         throw new SecurityException("Exe assembly must be strong named to produce a default UpdateStaus.xml file.");
      xml.originator = token;      
   }

   void CreateStatusStream(){      
      CreateDefaultXml();
      xmlFile = new FileStream(xmlFileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);                  
      FlushXml();

      // Hiding this file might be a nice touch
      //File.SetAttributes(xmlFileName, FileAttributes.Hidden);
   }

   void FlushXml(){
      xmlFile.Position = 0;
      ser.Serialize(xmlFile, xml);
      xmlFile.SetLength(xmlFile.Position);
   }
}

/// <summary>
/// XmlStatus: This is a simple type used by the XML serializer 
/// to read and write XML.
/// </summary>
[XmlRootAttribute("UpdateStatus", Namespace="", IsNullable=false)]
public class XmlStatus{   
   [XmlElementAttribute("Originator")]
   public String originator;   
   [XmlElementAttribute("NextUpdate")]
   public Int32 nextUpdate;
   [XmlElementAttribute("Patch")]
   public XmlPatch patch;   
   [XmlElementAttribute("CurrentBITS")]
   public String bitsJob;   

   public struct XmlPatch{
      [XmlAttributeAttribute("Name")]
      public String patchFilename;  
      [XmlAttributeAttribute("Url")]
      public String patchUrl;   
   }
}
