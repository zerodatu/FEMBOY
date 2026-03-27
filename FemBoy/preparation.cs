using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;

/// <summary>
/// 本作業を開始する前の準備処理（依存ソフトウェアのチェックなど）を管理するクラスです。
/// </summary>
public class Preparation
{
    /// <summary>
    /// 準備処理のメインエントリポイントです。
    /// ffmpeg がインストールされているか確認し、未インストールの場合は OS ごとのインストールコマンドを表示します。
    /// </summary>
    public static bool PreparationMain()
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

                return false;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("winget install Gyan.FFmpeg\n");
                Console.ResetColor();
                Console.WriteLine("If you are using winget for the first time, you will need to grant permission on your end.");

                return false;
            }
            else
            {
                // 現状MacOSをサポートしていないので、falseとしている。
                return false;
            }
        }
        else
        {
            if (IsCheckResource() == false)
            { return false; }
            else
            {
                Console.WriteLine("Resource check OK!\n");
                return true;
            }
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

    /// <summary>
    /// リソースチェックする関数
    /// </summary>
    /// <returns>
    /// bool:リソースが存在していればtrue / リソースが存在していなければfalse
    /// </returns>
    static bool IsCheckResource()
    {
        Const ConstInstance = new Const();

        // 音声ファイルあるかチェック
        bool has_audio = false;
        try
        {
            foreach (string audio_format in ConstInstance.AUDIO_FORMAT)
            {
                var files = Directory.EnumerateFiles(ConstInstance.MUSIC_DIR, audio_format, SearchOption.AllDirectories);
                if (files.Any())
                {
                    has_audio = true;
                    break;
                }
            }
        }
        catch
        {
            Console.WriteLine("No make Dir -> " + ConstInstance.MUSIC_DIR);
            return false;
        }
        if (!has_audio)
        {
            Console.WriteLine("No music file -> " + string.Join(", ", ConstInstance.AUDIO_FORMAT));
            return false;
        }

        // 画像ファイルがあるかチェック
        bool has_img = false;
        try
        {
            foreach (string img_format in ConstInstance.IMG_FORMAT)
            {
                var files = Directory.EnumerateFiles(ConstInstance.PIC_DIR, img_format, SearchOption.AllDirectories);
                if (files.Any())
                {
                    has_img = true;
                    break;
                }
            }
        }
        catch
        {
            Console.WriteLine("No make Dir -> " + ConstInstance.PIC_DIR);
            return false;
        }
        if (!has_img)
        {
            Console.WriteLine("No Pic file -> " + string.Join(", ", ConstInstance.IMG_FORMAT));
            return false;
        }
        return true;
    }


}
