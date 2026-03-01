use clap::Parser;

/// サンプルCLIアプリ
#[derive(Parser)]
#[command(
    name = "FEMBOY",
    about = "お手軽MV作成ソフトです",
    long_about = r#"
まず pic に写したい画像を入れてください

次に music に流したい楽曲を入れてね

最後に title.txt にフォーマットに沿って、作曲者と楽曲名を入れてね
"#
)]
struct Cli {
    /// デバッグモード
    #[arg(short, long)]
    debug: bool,
}

fn main() {
    let _cli = Cli::parse();
}
