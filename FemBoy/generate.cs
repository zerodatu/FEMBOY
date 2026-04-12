using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using System.Net;

/// <summary>
/// 動画生成処理を管理するクラスです。
/// </summary>
public class Generate
{
    /// <summary>
    /// ユーザーが選択したアップロードサイトに応じた動画生成処理のエントリポイントです。
    /// </summary>
    /// <param name="select">
    /// ユーザーが選択したアップロードサイトを表す整数値。
    /// </param>
    /// <returns>
    /// 動画生成処理が正常に完了した場合は true、何らかの理由で処理が失敗した場合は false を返します。
    /// </returns>
    public static bool GenerateMain(int select)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        switch (select)
        {
            case Const.UPLOAD_X:
                Console.WriteLine("X.com用の動画生成処理に入ります。");
                map = GetTitleInfo();
                break;
            case Const.UPLOAD_YOUTUBE:
                Console.WriteLine("Youtube.com用の動画生成処理に入ります。");
                map = GetTitleInfo();
                break;
        }

        // jsonから取得した曲のタイトル、アーティスト名、BPMをコンソールに表示する
        foreach (var item in map)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
        Console.WriteLine("-------------------");

        // テキストから透過PNGを生成
        CreateTextOverlay(map, select);

        // Picフォルダ内の画像を選択に応じてリサイズ・トリミングして出力
        ProcessImages(select);

        // tmp配下の画像2枚を合成してPic/background.pngを出力
        CreateBackgroundImage();


        Console.WriteLine("-------------------");
        Console.WriteLine("Encording...");

        // 音声ファイルをffmpegで連番処理するための下処理をする。wav形式に変換する。
        string? input_audio_file_path = GetAudioPath();
        Const constInstance = new Const();
        string output_audio_file_path = Path.Combine(constInstance.TEMP_DIR, "converted_audio.wav");
        if (input_audio_file_path != null)
        {
            Console.WriteLine($"Audio file found: {input_audio_file_path}");
            Console.WriteLine($"ぶってぇ音声入ってる!!: {input_audio_file_path}");
            Console.WriteLine($"Converting audio to suitable format for video encoding: {output_audio_file_path}");
            AudioConvert(input_audio_file_path, output_audio_file_path);
        }
        else
        {
            Console.WriteLine("Audio file not found. Skipping audio conversion.");
            Console.WriteLine("音声ファイルがないめう.");
            return false;
        }

        // 変換した音声ファイルをffmpegで連番処理して画像シーケンスを生成する
        Console.WriteLine("-------------------");
        Console.WriteLine("ヴィジュアライザーを動画に含めますか? (y/n)");
        Console.WriteLine("Do you want to include the visualizer in the video? (y/n)");
        Console.Write("Input: ");
        string? input = Console.ReadLine();
        string imageSequencePattern = string.Empty;

        if (string.IsNullOrEmpty(input) || (input.ToLower() != "y" && input.ToLower() != "n"))
        {
            Console.WriteLine("入力できない文字が入っています。yかnを入力してください。");
            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            return false;
        }
        else if (input.ToLower() == "y")
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("ヴィジュアライザーを動画に含めます。待っててね!");
            Console.WriteLine("Including visualizer in the video. Please wait!");
            imageSequencePattern = MakeImageSequencePattern(output_audio_file_path, select);
        }
        else
        {
            // Nothing Do. 
            // Noを選択した場合、この処理をしない。
        }


        // background.png と音楽ファイルを組み合わせて H.264 動画を書き出す
        if (!CreateH264Video(select, imageSequencePattern))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 曲情報のマップからImageSharpを使用して透過PNG画像を生成します。
    /// </summary>
    /// <param name="map">曲情報（Title, Artist, BPMなど）</param>
    /// <param name="select">アップロードサイトの識別子</param>
    static void CreateTextOverlay(Dictionary<string, string> map, int select)
    {
        // tmpフォルダの作成
        string tmpDir = Path.Combine(Environment.CurrentDirectory, "tmp");
        if (!Directory.Exists(tmpDir))
        {
            Directory.CreateDirectory(tmpDir);
        }

        // 解像度の設定
        int width = (select == Const.UPLOAD_X) ? 1080 : 1920;
        int height = (select == Const.UPLOAD_X) ? 1920 : 1080;

        // テキストの構築
        string title = map.GetValueOrDefault("SongName", "Unknown Title");
        string artist = map.GetValueOrDefault("ComposerName", "Unknown Artist");
        string text = $"{title}{Environment.NewLine}{artist}";

        // フォントの設定
        // UbuntuなどのLinux環境で文字化け（豆腐）を防ぐため、日本語対応フォントを含む複数の候補から検索します。
        string[] fontCandidates = {
            "Noto Sans CJK JP",     // Ubuntu (Japanese)
            "TakaoPGothic",         // Ubuntu (Japanese fallback)
            "DejaVu Sans",          // Linux Standard
            "Arial",                // Windows
            "Liberation Sans",      // Linux Standard
            "FreeSans"              // Linux Standard
        };

        FontFamily? family = null;
        foreach (var candidate in fontCandidates)
        {
            if (SystemFonts.Collection.TryGet(candidate, out FontFamily f))
            {
                family = f;
                break;
            }
        }

        if (family == null)
        {
            // 候補が見つからない場合、システムにインストールされている最初のフォントを使用
            foreach (var f in SystemFonts.Collection.Families) { family = f; break; }
        }

        Font font = family?.CreateFont(96, FontStyle.Bold) ?? throw new Exception("システムにフォントが見つかりません。fontconfig等を確認してください。");

        using (Image<Rgba32> image = new Image<Rgba32>(width, height))
        {
            image.Mutate(ctx =>
            {
                if (select == Const.UPLOAD_X)
                {
                    // X.com用: キャンバス全体を一度回転させてから描画し、再度回転させて戻すことでテキストを回転させます
                    // 1080x1920 (縦) -> 1920x1080 (横) に回転
                    ctx.Rotate(90f);

                    var options = new RichTextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        // 回転後の座標系(1920x1080)での配置位置を指定
                        // X: 中央(1920/2), Y: 上端から80px（これが元の左端からの距離になります）
                        Origin = new PointF(height / 2f, 80)
                    };
                    ctx.DrawText(options, text, Color.White);

                    // 1920x1080 -> 1080x1920 に戻す
                    ctx.Rotate(-90f);
                }
                else
                {
                    // Youtube用: 下側に中央揃えで配置
                    var options = new RichTextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Origin = new PointF(width / 2, height - 100)
                    };
                    ctx.DrawText(options, text, Color.White);
                }
            });

            string outputPath = Path.Combine(tmpDir, "overlay.png");
            image.SaveAsPng(outputPath);
            Console.WriteLine($"Overlay image created: {outputPath}");
        }
    }

    /// <summary>
    /// title.jsonから曲のタイトル、アーティスト名、BPMを取得する関数
    /// </summary>
    /// <returns>
    /// Dictionary<string, string>: title.jsonから取得した曲のタイトル、アーティスト名、BPMを格納した辞書
    /// </returns>
    static Dictionary<string, string> GetTitleInfo()
    {
        string formatPath = Path.Combine(Environment.CurrentDirectory, "title.json");
        string jsonString = File.ReadAllText(formatPath);
        var param = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);
        return param?["SoundInfo"] ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// tmp/Trimming.png と tmp/overlay.png を合成して Pic/background.png を出力します。
    /// </summary>
    static void CreateBackgroundImage()
    {
        string tmpDir = Path.Combine(Environment.CurrentDirectory, "tmp");
        string trimmingPath = Path.Combine(tmpDir, "Trimming.png");
        string overlayPath = Path.Combine(tmpDir, "overlay.png");

        if (!File.Exists(trimmingPath))
        {
            Console.WriteLine($"Trimming.png が存在しないため、background.png の作成をスキップします: {trimmingPath}");
            return;
        }

        if (!File.Exists(overlayPath))
        {
            Console.WriteLine($"overlay.png が存在しないため、background.png の作成をスキップします: {overlayPath}");
            return;
        }

        string picDir = Path.Combine(Environment.CurrentDirectory, "Pic");
        Directory.CreateDirectory(picDir);
        string outputPath = Path.Combine(picDir, "background.png");

        try
        {
            using (Image<Rgba32> background = Image.Load<Rgba32>(trimmingPath))
            using (Image<Rgba32> overlay = Image.Load<Rgba32>(overlayPath))
            {
                if (background.Width != overlay.Width || background.Height != overlay.Height)
                {
                    overlay.Mutate(ctx => ctx.Resize(background.Width, background.Height));
                }

                background.Mutate(ctx => ctx.DrawImage(overlay, new Point(0, 0), 1f));
                background.SaveAsPng(outputPath);
            }

            Console.WriteLine($"Background image created: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create background.png: {ex.Message}");
        }
    }

    /// <summary>
    /// Pic/background.png と Music 内の音楽ファイルを ffmpeg で組み合わせて H.264 動画を書き出します。
    /// </summary>
    /// <param name="select">アップロードサイトの識別子</param>
    /// <returns>動画の書き出しに成功した場合は true、失敗した場合は false。</returns>
    static bool CreateH264Video(int select, string pettern_path)
    {
        // Helper: try to get audio duration via ffprobe (seconds). Returns -1 on failure.
        static double GetMediaDuration(string mediaPath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.ArgumentList.Add("-v");
                psi.ArgumentList.Add("error");
                psi.ArgumentList.Add("-show_entries");
                psi.ArgumentList.Add("format=duration");
                psi.ArgumentList.Add("-of");
                psi.ArgumentList.Add("default=noprint_wrappers=1:nokey=1");
                psi.ArgumentList.Add(mediaPath);

                using var p = Process.Start(psi);
                if (p == null) return -1;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(2000);
                if (string.IsNullOrWhiteSpace(output)) return -1;
                if (double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double sec))
                {
                    return sec;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        Const constInstance = new Const();
        string backgroundPath = Path.Combine(constInstance.PIC_DIR, "background.png");
        if (!File.Exists(backgroundPath))
        {
            Console.WriteLine($"background.png が存在しないため、動画生成をスキップします: {backgroundPath}");
            return false;
        }

        string? audioPath = null;
        foreach (string audioFormat in constInstance.AUDIO_FORMAT)
        {
            var matches = Directory.GetFiles(constInstance.MUSIC_DIR, audioFormat);
            Array.Sort(matches, StringComparer.OrdinalIgnoreCase);
            if (matches.Length > 0)
            {
                audioPath = matches[0];
                break;
            }
        }

        if (audioPath == null)
        {
            Console.WriteLine("Musicフォルダ内に使用可能な音楽ファイルが存在しないため、動画生成をスキップします。");
            return false;
        }

        string targetDir = Path.Combine(Environment.CurrentDirectory, "target");
        Directory.CreateDirectory(targetDir);
        string outputName = (select == Const.UPLOAD_X) ? "output_x.mp4" : "output_youtube.mp4";
        string outputPath = Path.Combine(targetDir, outputName);

        try
        {
            var stdoutBuffer = new List<string>();
            var stderrBuffer = new List<string>();

            // Try to get duration of the audio to compute percent
            double durationSec = GetMediaDuration(audioPath);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardOutput = true, // ffmpeg -progress pipe:1 writes progress to stdout
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("-y");
            psi.ArgumentList.Add("-loop");
            psi.ArgumentList.Add("1");
            psi.ArgumentList.Add("-i");
            psi.ArgumentList.Add(backgroundPath);
            psi.ArgumentList.Add("-i");
            psi.ArgumentList.Add(audioPath);
            bool hasPattern = !string.IsNullOrEmpty(pettern_path);
            if (hasPattern)
            {
                int fps = (select == Const.UPLOAD_X) ? 30 : 60;
                string overlayX = "W-w";
                string overlayY = "(H-h)/2";
                psi.ArgumentList.Add("-framerate");
                psi.ArgumentList.Add(fps.ToString());
                psi.ArgumentList.Add("-i");
                psi.ArgumentList.Add(pettern_path);
                psi.ArgumentList.Add("-filter_complex");
                psi.ArgumentList.Add($"[2:v]transpose=cclock[viz];[0:v][viz]overlay={overlayX}:{overlayY}:shortest=1[v]");
            }
            psi.ArgumentList.Add("-map");
            psi.ArgumentList.Add(hasPattern ? "[v]" : "0:v");
            psi.ArgumentList.Add("-map");
            psi.ArgumentList.Add("1:a");
            psi.ArgumentList.Add("-c:v");
            psi.ArgumentList.Add("libx264");
            psi.ArgumentList.Add("-threads");
            psi.ArgumentList.Add("0");
            psi.ArgumentList.Add("-preset");
            psi.ArgumentList.Add("veryfast");
            psi.ArgumentList.Add("-tune");
            psi.ArgumentList.Add("stillimage");
            psi.ArgumentList.Add("-pix_fmt");
            psi.ArgumentList.Add("yuv420p");
            psi.ArgumentList.Add("-c:a");
            psi.ArgumentList.Add("aac");
            psi.ArgumentList.Add("-b:a");
            psi.ArgumentList.Add("192k");
            psi.ArgumentList.Add("-shortest");
            // request machine-parseable progress on stdout and silence regular stats
            psi.ArgumentList.Add("-nostats");
            psi.ArgumentList.Add("-progress");
            psi.ArgumentList.Add("pipe:1");
            psi.ArgumentList.Add(outputPath);

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine("ffmpeg プロセスの起動に失敗しました。");
                return false;
            }

            object locker = new object();
            double lastOutSec = 0;

            // helper to render simple console progress bar
            void PrintProgress(double outSec)
            {
                TimeSpan elapsed;
                if (durationSec <= 0)
                {
                    // unknown duration: show elapsed time only
                    elapsed = TimeSpan.FromSeconds(outSec);
                    Console.Write("\rEncoding: elapsed " + elapsed.ToString(@"hh\:mm\:ss") + "\t");
                    return;
                }
                double pct = Math.Min(1.0, Math.Max(0.0, outSec / durationSec));
                int width = 30;
                int filled = (int)Math.Round(pct * width);
                string bar = new string('#', filled) + new string('-', width - filled);
                elapsed = TimeSpan.FromSeconds(outSec);
                TimeSpan tot = TimeSpan.FromSeconds(durationSec);
                string esStr = elapsed.ToString(@"hh\:mm\:ss");
                string totStr = tot.ToString(@"hh\:mm\:ss");
                Console.Write($"\rEncoding: [{bar}] {pct:P0} {esStr}/{totStr}   ");
            }

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (locker)
                {
                    stdoutBuffer.Add(e.Data);
                    // ffmpeg -progress pipe:1 emits key=value lines; when 'out_time_ms' or 'out_time' appears we update
                    var line = e.Data;
                    if (line.StartsWith("out_time_ms="))
                    {
                        if (long.TryParse(line.Substring("out_time_ms=".Length), out long ms))
                        {
                            double sec = ms / 1000000.0; // out_time_ms is microseconds
                            lastOutSec = sec;
                            PrintProgress(sec);
                        }
                    }
                    else if (line.StartsWith("out_time="))
                    {
                        // format: HH:MM:SS[.micro]
                        var t = line.Substring("out_time=".Length).Trim();
                        if (TimeSpan.TryParse(t, out TimeSpan ts))
                        {
                            lastOutSec = ts.TotalSeconds;
                            PrintProgress(lastOutSec);
                        }
                    }
                    else if (line.StartsWith("progress=end"))
                    {
                        // finished
                        if (durationSec > 0)
                        {
                            PrintProgress(durationSec);
                        }
                        Console.WriteLine();
                    }
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (locker) { stderrBuffer.Add(e.Data); }
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("ffmpeg による動画生成に失敗しました。");
                string stderr = string.Join(Environment.NewLine, stderrBuffer);
                string stdout = string.Join(Environment.NewLine, stdoutBuffer);
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Console.WriteLine(stderr);
                }
                else if (!string.IsNullOrWhiteSpace(stdout))
                {
                    Console.WriteLine(stdout);
                }
                return false;
            }

            Console.WriteLine($"Video created: {outputPath}");
            Console.WriteLine($"ここに動画が書き出されたゾ: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"動画生成中に例外が発生しました: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Picフォルダ内の画像を選択に応じてリサイズ・中央トリミングして出力します。
    /// X.com: 出力 1080x1920（高さを基準にリサイズ、必要なら幅基準で拡大）、中央でクロップ
    /// Youtube: 出力 1920x1080（幅を基準にリサイズ、必要なら高さ基準で拡大）、中央でクロップ
    /// 出力先: tmp/Trimming.png に PNG で保存します。
    /// </summary>
    static void ProcessImages(int select)
    {
        string picDir = Path.Combine(Environment.CurrentDirectory, "Pic");
        if (!Directory.Exists(picDir))
        {
            Console.WriteLine("Picフォルダが見つかりません。処理をスキップします。");
            return;
        }

        string tmpDir = Path.Combine(Environment.CurrentDirectory, "tmp");
        Directory.CreateDirectory(tmpDir);
        string outPath = Path.Combine(tmpDir, "Trimming.png");

        Const constInstance = new Const();
        var exts = new HashSet<string>(
            Array.ConvertAll(constInstance.IMG_FORMAT, format => format.Replace("*", "")),
            StringComparer.OrdinalIgnoreCase);
        var files = Directory.GetFiles(picDir);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                var ext = Path.GetExtension(file);
                if (!exts.Contains(ext)) continue;
                if (string.Equals(Path.GetFileName(file), "background.png", StringComparison.OrdinalIgnoreCase)) continue;

                using (Image image = Image.Load(file))
                {
                    int origW = image.Width;
                    int origH = image.Height;

                    int targetW = (select == Const.UPLOAD_X) ? 1080 : 1920;
                    int targetH = (select == Const.UPLOAD_X) ? 1920 : 1080;

                    // 元画像が既にターゲット解像度に一致している場合はそのままコピー（トリミング/リサイズ不要）
                    if (origW == targetW && origH == targetH)
                    {
                        image.SaveAsPng(outPath);
                        Console.WriteLine($"Skipped resize/crop (already target size): {file} -> {outPath}");
                        return;
                    }

                    int resizeW, resizeH;

                    if (select == Const.UPLOAD_X)
                    {
                        // 優先: 高さを合わせる。幅が足りなければ幅基準で拡大する。
                        double scale = (double)targetH / origH;
                        resizeW = (int)Math.Round(origW * scale);
                        resizeH = targetH;
                        if (resizeW < targetW)
                        {
                            scale = (double)targetW / origW;
                            resizeW = targetW;
                            resizeH = (int)Math.Round(origH * scale);
                        }
                    }
                    else
                    {
                        // Youtube: 優先: 幅を合わせる。高さが足りなければ高さ基準で拡大する。
                        double scale = (double)targetW / origW;
                        resizeW = targetW;
                        resizeH = (int)Math.Round(origH * scale);
                        if (resizeH < targetH)
                        {
                            scale = (double)targetH / origH;
                            resizeH = targetH;
                            resizeW = (int)Math.Round(origW * scale);
                        }
                    }

                    image.Mutate(ctx => ctx.Resize(resizeW, resizeH));

                    // 中心でクロップ
                    int cropX = Math.Max(0, (resizeW - targetW) / 2);
                    int cropY = Math.Max(0, (resizeH - targetH) / 2);
                    var cropRect = new Rectangle(cropX, cropY, targetW, targetH);

                    using (Image cropped = image.Clone(ctx => ctx.Crop(cropRect)))
                    {
                        cropped.SaveAsPng(outPath);
                        Console.WriteLine($"Processed: {file} -> {outPath}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process {file}: {ex.Message}");
            }
        }

        Console.WriteLine("トリミング対象の画像が見つかりませんでした。");
    }

    /// <summary>
    /// ffmpeg を使用して、入力された音声ファイルを H.264 動画の音声トラックに適した形式（PCM 16-bit リニア、44.1kHz、ステレオ）に変換します。
    /// </summary>
    /// <param name="inputPath">Audio file path</param>
    /// <param name="outputPath">Output file path</param>
    static void AudioConvert(string inputPath, string outputPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("-y");
            psi.ArgumentList.Add("-i");
            psi.ArgumentList.Add(inputPath);
            psi.ArgumentList.Add("-c:a");
            psi.ArgumentList.Add("pcm_s16le");
            psi.ArgumentList.Add("-ar");
            psi.ArgumentList.Add("44100");
            psi.ArgumentList.Add("-ac");
            psi.ArgumentList.Add("2");
            psi.ArgumentList.Add(outputPath);

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine("ffmpeg プロセスの起動に失敗しました。");
                return;
            }

            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("ffmpeg による音声変換に失敗しました。");
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Console.WriteLine(stderr);
                }
                return;
            }

            Console.WriteLine($"Audio converted: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"音声変換中に例外が発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// Musicフォルダ内からffmpegで処理可能な最初の音声ファイルを検索し、そのパスを返します。
    /// </summary>
    /// <returns>音声ファイルのパス</returns>
    static string? GetAudioPath()
    {
        Const constInstance = new Const();
        if (!Directory.Exists(constInstance.MUSIC_DIR))
        {
            Console.WriteLine("Musicフォルダが見つかりません。");
            return null;
        }

        foreach (string audioFormat in constInstance.AUDIO_FORMAT)
        {
            string? filePath = Directory.EnumerateFiles(constInstance.MUSIC_DIR, audioFormat, SearchOption.AllDirectories).FirstOrDefault();
            if (filePath != null)
            {
                return Path.GetFullPath(filePath);
            }
        }

        Console.WriteLine("音声ファイルが見つかりません。");
        return null;
    }

    /// <summary>
    /// ffmpeg を使用して、変換された音声ファイルを基に、showfreqs のスペクトラムヴィジュアライザーを生成し、指定された解像度とフレームレートで連番の画像シーケンスを出力します。
    /// </summary>
    /// <param name="target_wav_path">連番画像にする元のwavファイルのパス</param>
    /// <param name="upload_selectsite_number">アップロード先サイト番号</param>
    /// <returns>画像シーケンスの出力パターン</returns>
    static string MakeImageSequencePattern(string target_wav_path, int upload_selectsite_number)
    {
        Const constInstace = new Const();
        string outputDir = Path.Combine(constInstace.TEMP_DIR, "frames");
        string output_pattern = Path.Combine(outputDir, "frame_%05d.png");
        Directory.CreateDirectory(outputDir);

        foreach (string existingFrame in Directory.GetFiles(outputDir, "frame_*.png"))
        {
            File.Delete(existingFrame);
        }

        int width = (upload_selectsite_number == Const.UPLOAD_X) ? 540 : 960;
        int height = (upload_selectsite_number == Const.UPLOAD_X) ? 1920 : 1080;
        int fps = (upload_selectsite_number == Const.UPLOAD_X) ? 30 : 60;

        // transpose 後の縦サイズが動画高と一致するよう、生成時の横幅に height を使う。
        // NCS っぽい「帯域ごとのレベルがその場で上下する」見え方にするため、showfreqs のバー表示を使う。
        // 今のバランスを保ちつつ、FFT サイズを上げて帯域の粒度だけ細かくする。
        string filter = $"aformat=channel_layouts=mono,volume=3.4,showfreqs=s={height}x{width}:r={fps}:mode=bar:ascale=sqrt:fscale=log:win_size=16384:win_func=bharris:overlap=0.94:averaging=0.58:cmode=combined:minamp=0.000001:colors=0xeafcff,format=rgba,colorkey=black:0.18:0.04";
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("-y");
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add(target_wav_path);
        psi.ArgumentList.Add("-filter_complex");
        psi.ArgumentList.Add(filter);
        psi.ArgumentList.Add(output_pattern);

        using var process = Process.Start(psi);
        if (process == null)
        {
            Console.WriteLine("ffmpeg プロセスの起動に失敗しました。");
            return output_pattern;
        }

        string stderr = process!.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(stderr);
        Console.WriteLine($"ExitCode: {process.ExitCode}");

        return output_pattern;

    }

}
