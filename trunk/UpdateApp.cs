///////////////////////////////////////////////////////////////////////////////
//UpdateApp.cs
//Short application that is capable of automatically updating itself.
//
//Copyright (c)  Jason Clark  
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

class App{
   public static void Main(){
      // Check application update status
      UpdateStatus status = AutoUpdater.GetUpdateStatus(false);      
      // Change the "false" above to "true" to improve app startup speed
      // the number of relaunches necessary to see an update will
      // increase by one

      // If an update is available, prompt the user to apply
      if(status == UpdateStatus.ApplyUpdate && DialogResult.Yes == 
         MessageBox.Show(updateText, "Update Available", MessageBoxButtons.YesNo)){
         // Apply update and relaunch exe
         try{
            AutoUpdater.ApplyUpdate();
         }catch(System.Security.SecurityException){
            MessageBox.Show(updateErrorText, "Update Error");
         }
         AutoUpdater.RelaunchExe(true);
      }else
         Application.Run(new AppForm());
   }

   // Text with which to prompt user
   static String updateText = 
      "An application update is availalble, would you like to apply the update?";
   static String updateErrorText = 
      "The signature on the update did not check out!  Update not installed.";
}

/// <summary>
/// Summary description for AppForm.
/// </summary>
public class AppForm : System.Windows.Forms.Form {
   private PictureBox picture;
   private Label label;

#if V2   
   Image[] images;
   Int32 currentImage;
#endif
   
   public AppForm() {      
      InitializeComponent();   
      
      String[] gifs = Directory.GetFiles(".", "*.gif");
      if(gifs.Length != 0){
         picture.Image = new Bitmap(gifs[0]);
#if V2
         images = new Image[gifs.Length];
         for(Int32 index=0, end=gifs.Length; index<end; index++){
            images[index] = new Bitmap(gifs[index]);
         }

         currentImage = 0;

         Timer t = new Timer();
         t.Interval = 1500;
         t.Tick += new EventHandler(OnTimer);  
         t.Enabled = true;      
#endif

#if V2
         label.Text = "This is AutoUpdateApp V2.0--\nV2 shows a slide-show of the gifs in your directory.";
#endif
      }      
   }

#if V2
   void OnTimer(Object sender, EventArgs args){
      for(Int32 index=0, end=images.Length; index<end; index++){
         currentImage++;
         currentImage%=end;
         if(images[currentImage] != null){
            picture.Image = images[currentImage];            
            break;
         }
      }
   }
#endif

   #region Windows Form Designer generated code   
   private void InitializeComponent() {     
      this.label = new System.Windows.Forms.Label();
      this.picture = new System.Windows.Forms.PictureBox();
      this.SuspendLayout();
      // 
      // label
      // 
      this.label.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.label.Dock = System.Windows.Forms.DockStyle.Top;
      this.label.Font = new System.Drawing.Font("Comic Sans MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label.ForeColor = System.Drawing.Color.Blue;
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(336, 72);
      this.label.TabIndex = 0;
      this.label.Text = "This is AutoUpdateApp V1.0--\nThis application simply displays a bitmap in the cli" +
         "ent area.";
      // 
      // picture
      // 
      this.picture.Dock = System.Windows.Forms.DockStyle.Fill;
      this.picture.Location = new System.Drawing.Point(0, 72);
      this.picture.Name = "picture";
      this.picture.Size = new System.Drawing.Size(336, 502);
      this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.picture.TabIndex = 1;
      this.picture.TabStop = false;
      // 
      // AppForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(336, 574);
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                   this.picture,
                                                                   this.label});
      this.Name = "AppForm";
      this.Text = "AutoUpdate Sample Application";
      this.ResumeLayout(false);

   }
   #endregion
}