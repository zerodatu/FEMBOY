#!/bin/bash
set -euo pipefail

# 安全に出力ディレクトリを掃除してから publish を実行する
rm -rf ./dist/win ./dist/linux ./dist/osx-arm64 ./dist/osx-x64 || true
mkdir -p ./dist/win ./dist/linux ./dist/osx-arm64 ./dist/osx-x64

echo "Publishing win-x64 to ./dist/win"
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/win

echo "Publishing linux-x64 to ./dist/linux"
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/linux

echo "Publishing osx-arm64 to ./dist/osx-arm64"
dotnet publish -r osx-arm64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/osx-arm64
mv ./dist/osx-arm64/FemBoy ./dist/osx-arm64/FemBoy-arm64

echo "Publishing osx-x64 to ./dist/osx-x64"
dotnet publish -r osx-x64 -c Release -p:PublishSingleFile=true --self-contained true -o ./dist/osx-x64
mv ./dist/osx-x64/FemBoy ./dist/osx-x64/FemBoy-x64