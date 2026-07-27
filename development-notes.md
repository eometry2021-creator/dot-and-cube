# 开发记录 Development Notes
## 2026/07/21
- 创建仓库。上传了已经完成的 4 种 Dot 类障碍物脚本与其他资源库。
## 2026/07/23
- 目录重构 `script/` → `scripts/`，`scene/` → `scenes/`；新增 `levels/` 目录；
- 提取 `DotBase` 抽象基类；
- 四个子类继承 DotBase 并简化。

鉴于创建仓库的目的之一就是长期维护该项目，我便决定采用一套**文件命名规范**。<br>
`C#` 类/源文件采用 PascalCase（大驼峰命名法），原因：
- 服从 .NET 官方规范。
文件夹、Godot 场景、美术与声音资源采用 snake_case（蛇形命名法），<br>
如有需要可以采用带有前缀的 Hungarian Notation（匈牙利命名法），原因：
- 避免跨平台大小写敏感冲突；
- 工具链与资源检索友好。
文件夹命名时采用的复数规则：
- 资源集合（表意为：这里面存放的是很多个 XXX），根据名词是否可数添加复数。
- 概念分类（表意为：某一特定系统，如障碍物、玩家），一律采用单数。

## 2026/07/24
- 更新关卡场景 `test_room`，调试证明所有的 `Dot` 类均能正常运行；
- 修复了 `DotEllipse` 中 `Period` 未赋值给 `_period` 导致引擎内部归一化计算失败的 bug。

## 2026/07/27
- 新增玩家系统：PlayerVelocity；
- Dot 基类下新增 GrazeCircle（擦弹圈）机制；
- 新增 Dot 类：DotPetal（暂为框架，未完成调试）；
- 新建 DEVELOPMENT_NOTES.md。