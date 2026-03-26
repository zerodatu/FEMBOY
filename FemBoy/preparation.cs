using System;
using System.Diagnostics;

// 本作業前の準備処理の実装
public class Preparation
{
    public static void PreparationMain()
    {
        bool installed = IsFfmpegInstalled();
        Console.WriteLine(installed ? "You alrady installed the ffmpeg." : "You don't install the ffmpeg.");
        if(installed == false)
        {
            
        }
    }
    static async Task Download_ffmpeg()
    {
        Console.WriteLine("適当な文字列");
    }
    static bool IsFfmpegInstalled()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return false;
            }
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}