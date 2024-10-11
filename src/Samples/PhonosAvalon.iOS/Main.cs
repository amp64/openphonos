using UIKit;
using System;
using Microsoft.Extensions.Logging;

namespace PhonosAvalon.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        try
        {
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
        catch (Exception ex)
        {
            Console.Write("\nFATAL ERROR: ");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (App.AppLoggerFactory != null)
            {
                var logger = App.AppLoggerFactory.CreateLogger("App");
                logger.LogCritical(ex, "Fatal error in Main()");
            }
        }
    }
}
