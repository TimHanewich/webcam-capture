using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using TimHanewich.Toolkit;
using Newtonsoft.Json;

namespace WebcamCapture
{
    public class Program
    {
        public static void Main(string[] args)
        {

            

            Webcam[]? webcams = JsonConvert.DeserializeObject<Webcam[]>(System.IO.File.ReadAllText(@"C:\Users\timh\Downloads\tah\webcam-capture\webcams.json"));

            StorageOrchestrator orch = new StorageOrchestrator();
            if (webcams != null)
            {
                foreach (Webcam wb in webcams)
                {
                    wb.SetNameFromUrl();
                }
                orch.AddWebcams(webcams);
            }
            orch.RootDirectory = @"C:\Users\timh\Downloads\webcams";
            orch.StatusUpdate += PrintStatus;


            while (true)
            {
                orch.ExecuteRoundAsync().Wait();

                Console.Write("Waiting 5 seconds to check again...");
                System.Threading.Tasks.Task.Delay(new TimeSpan(0, 0, 5)).Wait();
                Console.WriteLine("Proceeding!");
            }
            
            
        }


        public static void PrintStatus(string status)
        {
            Console.WriteLine(status);
        }
    }
}