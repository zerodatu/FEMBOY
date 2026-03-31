using System;
class FemBoy
{
    static void Main(String[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine("使い方(日本語版)" + Environment.NewLine +
                "1.Picフォルダに画像を入れる。" + Environment.NewLine +
                "2.Musicフォルダに使いたい音楽を入れる" + Environment.NewLine +
                "3.format.txtにアーティスト情報と曲名とBPMを入れる" + Environment.NewLine +
                "4.準備が完了したら 再度実行する");
            Console.WriteLine("---------");
            Console.WriteLine("How to use(English version)" + Environment.NewLine +
                "1. Put images in the Pic folder." + Environment.NewLine +
                "2. Put the music files you want to use in the Music folder." + Environment.NewLine +
                "3. Enter the artist name, song title, and BPM in format.txt." + Environment.NewLine +
                "4. Once the preparation is complete, run the program again.");
            Console.WriteLine("---------");
        }
        else if (args.Length == 0)
        {
            // ffmpegがインストールされているかチェックして、未インストールならインストールを行う
            bool resource_flg = Preparation.PreparationMain();

            if (!resource_flg)
            {
                return;
            }

            Console.WriteLine("-------------------");
            Console.WriteLine(@"███████╗███████╗███╗   ███╗██████╗  ██████╗ ██╗   ██╗");
            Console.WriteLine(@"██╔════╝██╔════╝████╗ ████║██╔══██╗██╔═══██╗╚██╗ ██╔╝");
            Console.WriteLine(@"█████╗  █████╗  ██╔████╔██║██████╔╝██║   ██║ ╚████╔╝ ");
            Console.WriteLine(@"██╔══╝  ██╔══╝  ██║╚██╔╝██║██╔══██╗██║   ██║  ╚██╔╝  ");
            Console.WriteLine(@"██║     ███████╗██║ ╚═╝ ██║██████╔╝╚██████╔╝   ██║   ");
            Console.WriteLine(@"╚═╝     ╚══════╝╚═╝     ╚═╝╚═════╝  ╚═════╝    ╚═╝   ");
            Console.WriteLine("-------------------");

            // ユーザー側でアップロードサイトを選択して、入力情報に問題がないか確認
            bool select_flg = UserSelect.SelectUser();

            if (!select_flg)
            {
                return;
            }


            // 動画生成処理に入る
            bool generate_flg = Generate.GenerateMain(Shared.select_site_num);

            if (!generate_flg)
            {
                return;
            }

            Console.WriteLine("-------------------");
            Console.WriteLine("処理が完了したゾ");
            Console.WriteLine("Finished processing");
            Console.WriteLine("-------------------");


        }
        else
        {
            Console.WriteLine("Do it --help!");
        }
    }
}