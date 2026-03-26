using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// 本作業を開始する前の準備処理（依存ソフトウェアのチェックなど）を管理するクラスです。
/// </summary>
public class Preparation
{
    /// <summary>
    /// 準備処理のメインエントリポイントです。
    /// ffmpeg がインストールされているか確認し、未インストールの場合は OS ごとのインストールコマンドを表示します。
    /// </summary>
    public static void PreparationMain()
    {
        bool installed = IsFfmpegInstalled();
        Console.WriteLine(installed ? "You alrady installed the ffmpeg.\n" : "You don't install the ffmpeg.\n");
        if (installed == false)
        {
            Console.WriteLine("Please execute this command.\n");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("sudo apt update && sudo apt install ffmpeg");
                Console.ResetColor();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("winget install Gyan.FFmpeg\n");
                Console.ResetColor();
                Console.WriteLine("If you are using winget for the first time, you will need to grant permission on your end.");
            }
        }
        else
        {
            Console.WriteLine(@"███████╗███████╗███╗   ███╗██████╗  ██████╗ ██╗   ██╗");
            Console.WriteLine(@"██╔════╝██╔════╝████╗ ████║██╔══██╗██╔═══██╗╚██╗ ██╔╝");
            Console.WriteLine(@"█████╗  █████╗  ██╔████╔██║██████╔╝██║   ██║ ╚████╔╝ ");
            Console.WriteLine(@"██╔══╝  ██╔══╝  ██║╚██╔╝██║██╔══██╗██║   ██║  ╚██╔╝  ");
            Console.WriteLine(@"██║     ███████╗██║ ╚═╝ ██║██████╔╝╚██████╔╝   ██║   ");
            Console.WriteLine(@"╚═╝     ╚══════╝╚═╝     ╚═╝╚═════╝  ╚═════╝    ╚═╝   ");
        }
    }
    /// <summary>
    /// システムに ffmpeg がインストールされ、パスが通っているかを確認します。
    /// </summary>
    /// <returns>インストールされている場合は true、それ以外の場合は false。</returns>
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