using System;
using System.IO;

namespace PieDb.FileSystem
{
    internal static class Helpers
    {
        internal static string GetAppDataPath()
        {
            return AppDomain.CurrentDomain.GetData("DataDirectory") as string ?? Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "App_Data");
        }
    }
}