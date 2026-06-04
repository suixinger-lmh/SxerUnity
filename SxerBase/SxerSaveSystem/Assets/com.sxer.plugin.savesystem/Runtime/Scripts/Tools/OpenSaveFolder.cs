
using System.Diagnostics;
using UnityEngine;

namespace Sxer.Plugin.SaveSystem.Tool
{
    public static class OpenSaveFolder
    {
        public static readonly string filePath = Application.persistentDataPath;

        public static void OpenFolder() {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}