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

        // background.png と音楽ファイルを組み合わせて H.264 動画を書き出す
        if (!CreateH264Video(select))
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
    static bool CreateH264Video(int select)
    {
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
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
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
            psi.ArgumentList.Add(outputPath);

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine("ffmpeg プロセスの起動に失敗しました。");
                return false;
            }

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("ffmpeg による動画生成に失敗しました。");
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
}
