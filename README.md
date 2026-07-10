# ToDo 管理画面

.NET Framework 4.8 + Windows Forms + NUnit 3.14 で構築した ToDo 管理アプリケーションです。

## プロジェクト構成

```
TodoApp.sln
├── src/TodoApp.Core           ドメインロジックと永続化
├── src/TodoApp.WinForms       Windows Forms 画面
└── src/TodoApp.Core.Tests     NUnit 単体テスト
```

## 必要環境

- .NET Framework 4.8 Runtime
- MSBuild 4.0
- NuGet CLI（テスト実行時にパッケージを復元する場合）

## ビルド

ソリューションをビルドします。

```cmd
MSBuild TodoApp.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

初回ビルド前に NuGet パッケージを復元する場合は以下を実行してください。

```cmd
nuget restore TodoApp.sln
```

## 実行

ビルド後、以下の実行ファイルを起動します。

```
src/TodoApp.WinForms/bin/Debug/TodoApp.WinForms.exe
```

## テスト

NUnit.ConsoleRunner を使用してテストを実行します。

```cmd
packages\NUnit.ConsoleRunner.3.16.3\tools\nunit3-console.exe src\TodoApp.Core.Tests\bin\Debug\TodoApp.Core.Tests.dll
```

## 機能

- タスクの追加/編集/削除
- 完了/未完了の切り替え
- ステータス（全て/未完了/完了）によるフィルタ
- JSON ファイルへの永続化

## ドキュメント

- `docs/01_調査影響分析.md`
- `docs/02_設計書.md`
- `docs/03_E2Eテスト仕様書.md`
