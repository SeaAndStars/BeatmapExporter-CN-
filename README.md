# BeatmapExporter（用于osu!lazer）

### 支持开发者

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

如有问题或需要更新，您可以在GitHub上创建问题。或者，您可以通过Discord找到我，通过[我的机器人的支持服务器](https://discord.com/invite/ucVhtnh)联系我。尽管它不是专门针对BeatmapExporter，但我不介意它被用于我的其他实用程序，比如这个。

# 目的/功能

BeatmapExporter是一个命令行程序/工具，可以从现代osu！lazer存储格式中批量导出您的osu！谱库。

osu！lazer没有像“稳定”版osu！那样的“Songs/”文件夹。 Lazer的文件存储在您PC上的散列文件名下，有关谱面的其他信息包含在本地的“Realm”数据库中。

## 谱面导出

这种新的存储格式在玩游戏时带来了更好的体验。然而，这个系统的一个结果是，您不能轻松地导出您的所有或部分歌曲库以供共享或迁回osu！稳定版。

该实用程序允许您将谱面重新导出为`.osz`文件。

有一个谱面筛选系统，允许您选择库的一部分仅导出特定的谱面（例如，超过一定星级的，特定的艺术家/制图师，特定的游戏模式，特定的集合等）。您还可以一次导出**整个库**。

您还可以直接导出为.zip，更轻松地转移您的库。

## 音频导出

从1.2版本开始，有一个选项只导出音频文件。与整个谱面存档不同，只会导出.mp3音频文件。

.mp3文件带有基本的艺术家/歌曲信息，并在可能的情况下嵌入了来自osu！的背景文件。

如果谱面使用的是非mp3音频格式，[FFmpeg](https://ffmpeg.org/download.html)将需要进行转码为mp3。在启动BeatmapExporter之前，可以将ffmpeg.exe（适用于Windows）放在系统PATH上，或者简单地放在BeatmapExporter.exe旁边。

## 背景图像导出

从1.3.8版本开始，有一个选项只导出[谱面背景图像文件](https://github.com/kabiiQ/BeatmapExporter/pull/10)。

# 下载/使用

可以从GitHub的[Releases](https://github.com/kabiiQ/BeatmapExporter/releases)部分下载可执行文件。

如果您使用的是Windows系统，并且osu！数据库位于默认位置（%appdata%\osu），您应该可以直接运行该应用程序。如果您在安装osu！lazer时更改了数据库位置，则该程序将无法定位它，并将提示您输入位置。

如果您不在Windows上，我为OSX和Linux提供了默认目录，并且它应该自动工作，但没有经过测试。

您还可以将数据库文件夹作为启动参数启动程序，如果您已经知道它将位于不寻常的位置。所需的数据库文件夹包含一个“files”文件夹。如果您将其移动并不确定其位置，可以在游戏内打开此文件夹。如果没有移动它，它应该会自动工作。

# 基本导出任务截图（使用标签导出谱面）

![](https://i.imgur.com/bbM1D5Z.png)