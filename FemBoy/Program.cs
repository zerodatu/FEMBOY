using System;
class FemBoy
{
    static void Main(String[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine("使い方" + Environment.NewLine +
                "1.Picフォルダに画像を入れる。" + Environment.NewLine +
                "2.Musicフォルダに使いたい音楽を入れる" + Environment.NewLine +
                "3.format.txtにアーティスト情報と曲名とBPMを入れる" + Environment.NewLine +
                "4.準備が完了したら femboy --comeshotで実行する");
        }
        else if (args.Contains("--comeshot"))
        {
            // ffmpegがインストールされているかチェックして、未インストールならインストールを行う
            bool resource_flg = Preparation.PreparationMain();

            if (!resource_flg)
            {
                return;
            }

            Console.WriteLine(@"███████╗███████╗███╗   ███╗██████╗  ██████╗ ██╗   ██╗");
            Console.WriteLine(@"██╔════╝██╔════╝████╗ ████║██╔══██╗██╔═══██╗╚██╗ ██╔╝");
            Console.WriteLine(@"█████╗  █████╗  ██╔████╔██║██████╔╝██║   ██║ ╚████╔╝ ");
            Console.WriteLine(@"██╔══╝  ██╔══╝  ██║╚██╔╝██║██╔══██╗██║   ██║  ╚██╔╝  ");
            Console.WriteLine(@"██║     ███████╗██║ ╚═╝ ██║██████╔╝╚██████╔╝   ██║   ");
            Console.WriteLine(@"╚═╝     ╚══════╝╚═╝     ╚═╝╚═════╝  ╚═════╝    ╚═╝   ");
        }
        else
        {
            Console.WriteLine("Do it --help!");
        }
    }
}