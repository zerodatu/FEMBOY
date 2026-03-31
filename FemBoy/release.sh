#!/bin/bash
set -euo pipefail

# 安全に出力ディレクトリを掃除してから publish を実行する
rm -rf ./dist/win ./dist/linux || true
mkdir -p ./dist/win ./dist/linux

echo "Publishing win-x64 to ./dist/win"
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/win

echo "Publishing linux-x64 to ./dist/linux"
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/linux