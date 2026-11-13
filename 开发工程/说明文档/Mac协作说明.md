# Mac 与 Windows 一起开发

Mac 同学使用仓库里的 **开发工程**。Windows游戏.zip 是 Windows 试玩程序，不是开发工程。

## 第一次打开

1. 安装 Mac 版 Unity Hub，按电脑芯片选择相应的 Unity Editor 架构。
2. 安装项目对应版本 **6000.6.5f1**。不要自行升级工程版本。若 Hub 列表找不到，用 Unity 官方下载档案定位该版本。
3. 用 GitHub Desktop 或 Git 克隆整个 507games 仓库；克隆比反复下载源码 ZIP 更方便提交修改。
4. 在 Unity Hub 选择“添加现有项目”，选中仓库中的 **开发工程** 文件夹。正确文件夹里应当同时有 Assets、Packages、ProjectSettings。
5. 第一次等待 Unity 在自己电脑上生成 Library 并导入素材。
6. 打开 `Assets/Scenes/DodgeStudy.unity`，按 Play，再按 Enter 开始。地下场景自动加载，不要只运行 LowerPassage。

可以在 Mac 编辑器里修改场景、脚本、美术和动画，也可以直接 Play 测试。无需运行 Windows exe。

## 字体和构建

界面字体按可用字体选择，包含 Windows 的微软雅黑、Mac 的苹方和黑体，以及通用字体后备。

保存场景并停止 Play 后，菜单“白模工作台 → 构建试玩（保留现有场景）”按当前编辑器系统选择输出：Mac 为 `Build/DodgeStudy.app`，Windows 为 `Build/DodgeStudy.exe`。如果提示缺少支持，在 Unity Hub 中给同一版本编辑器添加对应桌面构建模块。日常共同开发并不需要每次生成独立程序。

## 六人如何同步

- 全员使用同一个 Unity 编辑器版本。
- 各自使用分支，完成任务后提交 Pull Request 合并。
- 每次开工先同步。两位地图场景同学尽量错开同一个场景文件的编辑，避免互相覆盖。
- 在 Unity 的 Project 窗口移动素材，让 `.meta` 引用一起保留。素材和对应 `.meta` 一起提交。
- 不提交 Library、Temp、Logs、UserSettings 和 Build。这些已由仓库忽略配置排除。
- 不把某台电脑的绝对路径写入脚本，文件名大小写保持一致。
- 美术替换视觉子节点，保留平台、升降台、地刺、标记等玩法根节点。

## Mac 首次验收

请一位 Mac 同学从干净克隆开始，检查：脚本无编译错误、中文能显示、主场景能 Play、地下能加载、升降台能乘坐、Boss 和剧情道具返回正常。独立 `.app` 构建也应在 Mac 上试一次。

当前代码已补字体后备与按系统选择构建目标。维护端只有 Windows；Windows 构建和运行验证不能代替 Mac 实机验收。

官方说明：[Unity Hub 添加项目](https://docs.unity.com/en-us/hub/project-manage)、[安装编辑器](https://docs.unity.com/en-us/hub/add-editor)、[字体后备接口](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/font/createdynamicfontfromosfont)。
