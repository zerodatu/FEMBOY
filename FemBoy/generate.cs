using System;
using System.Collections.Generic;
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

        Font font = family?.CreateFont(50, FontStyle.Bold) ?? throw new Exception("システムにフォントが見つかりません。fontconfig等を確認してください。");

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
    /// Picフォルダ内の画像を選択に応じてリサイズ・中央トリミングして出力します。
    /// X.com: 出力 1080x1920（高さを基準にリサイズ、必要なら幅基準で拡大）、中央でクロップ
    /// Youtube: 出力 1920x1080（幅を基準にリサイズ、必要なら高さ基準で拡大）、中央でクロップ
    /// 出力先: tmp/processed/X または tmp/processed/Youtube に PNG で保存します。
    /// </summary>
    static void ProcessImages(int select)
    {
        string picDir = Path.Combine(Environment.CurrentDirectory, "Pic");
        if (!Directory.Exists(picDir))
        {
            Console.WriteLine("Picフォルダが見つかりません。処理をスキップします。");
            return;
        }

        string outSub = (select == Const.UPLOAD_X) ? "X" : "Youtube";
        string outDir = Path.Combine(Environment.CurrentDirectory, "tmp", "processed", outSub);
        Directory.CreateDirectory(outDir);

        var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
        var files = Directory.GetFiles(picDir);

        foreach (var file in files)
        {
            try
            {
                var ext = Path.GetExtension(file);
                if (!exts.Contains(ext)) continue;

                using (Image image = Image.Load(file))
                {
                    int origW = image.Width;
                    int origH = image.Height;

                    int targetW = (select == Const.UPLOAD_X) ? 1080 : 1920;
                    int targetH = (select == Const.UPLOAD_X) ? 1920 : 1080;

                    // 元画像が既にターゲット解像度に一致している場合はそのままコピー（トリミング/リサイズ不要）
                    if (origW == targetW && origH == targetH)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        string outPath = Path.Combine(outDir, name + ".png");
                        image.SaveAsPng(outPath);
                        Console.WriteLine($"Skipped resize/crop (already target size): {file} -> {outPath}");
                        continue;
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
                        string name = Path.GetFileNameWithoutExtension(file);
                        string outPath = Path.Combine(outDir, name + ".png");
                        cropped.SaveAsPng(outPath);
                        Console.WriteLine($"Processed: {file} -> {outPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process {file}: {ex.Message}");
            }
        }
    }
}