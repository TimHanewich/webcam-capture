using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TimHanewich.Toolkit;

namespace WebcamCapture
{
    public class StorageOrchestrator
    {
        public event Notify StatusUpdate;
        public string RootDirectory {get; set;}

        private List<Webcam> Webcams;

        public StorageOrchestrator()
        {
            RootDirectory = string.Empty;
            Webcams = new List<Webcam>();
        }

        public void AddWebcam(Webcam cam)
        {
            Webcams.Add(cam);
        }

        public void AddWebcams(Webcam[] cams)
        {
            foreach (Webcam wc in cams)
            {
                AddWebcam(wc);
            }
        }
    
        public void CreateDirectories()
        {
            foreach (Webcam cam in Webcams)
            {
                string path = System.IO.Path.Combine(RootDirectory, cam.Name);
                if (System.IO.Directory.Exists(path) == false)
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
        }
    
        public async Task ExecuteRoundAsync()
        {
            CreateDirectories();

            foreach (Webcam cam in Webcams)
            {
                UpdateStatus("Checking camera '" + cam.Name + "'");
                if (cam.Check())
                {
                    UpdateStatus("Camera '" + cam.Name + "' reached time.");
                    UpdateStatus("Downloading from camera '" + cam.Name + "'...");

                    try
                    {
                        Stream s = await cam.DownloadAsync();
                        UpdateStatus("Downloaded with " + s.Length.ToString() + " bytes");

                        //Is it new?
                        bool is_new = cam.IsNew(s);
                        if (is_new)
                        {
                            UpdateStatus("It is a new image for '" + cam.Name + "'!");

                            //set last received
                            cam.SetLastReceived(s);
                            s.Seek(0, SeekOrigin.Begin);

                            //Save to file
                            string directory = System.IO.Path.Combine(RootDirectory, cam.Name);
                            string new_file_path = System.IO.Path.Combine(directory, new HanewichTimeStamp(DateTime.UtcNow).ToString() + ".jpg");
                            Stream new_file = System.IO.File.Create(new_file_path);
                            s.CopyTo(new_file);
                            new_file.Close();
                            s.Close();

                            //Print status
                            UpdateStatus("Data from cam '" + cam.Name + "' written to '" + new_file_path + "'");
                        }
                        else
                        {
                            UpdateStatus("Image for '" + cam.Name + "' was not new!");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus("FAILURE ENCOUNTERED ON CAM '" + cam.Name + "'! Msg: " + ex.Message);
                    }  
                }
            }

        }


        private void UpdateStatus(string status)
        {
            try
            {
                StatusUpdate.Invoke(status);
            }
            catch
            {

            }
        }


    }
}