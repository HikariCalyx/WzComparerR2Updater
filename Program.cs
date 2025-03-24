﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            return;
        }

        string updateZipPath = args[0];
        string dotNetVersion = args[1];
        string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string tempUpdateFolder = Path.Combine(currentDirectory, "UpdateTemp");
        string wzComparerR2Path = Path.Combine(currentDirectory, "WzComparerR2.exe");

        string targetSubFolder = GetTargetSubFolder(dotNetVersion);

        if (string.IsNullOrEmpty(targetSubFolder)) return;

        try
        {
            KillProcess("WzComparerR2");

            if (Directory.Exists(tempUpdateFolder))
            {
                Directory.Delete(tempUpdateFolder, true);
            }
            Directory.CreateDirectory(tempUpdateFolder);
            ZipFile.ExtractToDirectory(updateZipPath, tempUpdateFolder);

            string[] directoriesToDelete = { "Lib", "Plugin", "runtimes" };
            foreach (var dir in directoriesToDelete)
            {
                string dirPath = Path.Combine(currentDirectory, dir);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
            }

            foreach (var file in Directory.GetFiles(currentDirectory, "WzComparerR2*"))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(Path.Combine(tempUpdateFolder, targetSubFolder)))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(currentDirectory, fileName);
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
                File.Move(file, destFile);
            }

            MoveDirectory(Path.Combine(tempUpdateFolder, targetSubFolder), currentDirectory);
            Directory.Delete(tempUpdateFolder, true);

        }
        catch (Exception ex)
        {
            MessageBox.Show("Update failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (File.Exists(updateZipPath))
            {
                File.Delete(updateZipPath);
            }
            if (Directory.Exists(tempUpdateFolder))
            {
                Directory.Delete(tempUpdateFolder, true);
            }
        }
        finally
        {

            Process.Start(wzComparerR2Path);
        }
    }

    static void KillProcess(string processName)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            process.Kill();
            process.WaitForExit();
        }
    }

    static string GetTargetSubFolder(string versionArg)
    {
        switch (versionArg)
        {
            case "4": return "net462";
            case "6": return "net6.0-windows";
            case "8": return "net8.0-windows";
            default: return null;
        }
    }

    static void MoveDirectory(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(targetDir, fileName);
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
            File.Move(file, destFile);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destDir = Path.Combine(targetDir, dirName);
            MoveDirectory(subDir, destDir);
        }
    }
}