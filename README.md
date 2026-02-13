# DotArsenal v1.01 (基于Unity引擎开发)

一个基于 "Dot"（点阵/像素点）主题的 Roguelite 2D俯视角动作游戏。玩家需要收集敌人掉落的 Dot 资源，在 5x5 的网格中绘制图案来合成强大的武器，击败太空地牢中的强敌。

![Banner Image Placeholder](Docs/Images/Banner.png)
*(在此处放置游戏主视觉图)*

## 🎮 游戏核心机制 (Core Mechanics)

### 1. Dot 收集与合成 (Collect & Craft)
游戏的核心循环围绕着 **Dot** 资源展开。
- **收集**: 击败敌人会掉落 Dot。
- **合成**: 按下 `Tab` 键打开 5x5 的组装面板。
- **绘制**: 消耗 Dot 在网格上绘制特定的图案（Pattern）。
- **装备**: 如果图案匹配已知的武器配方（Weapon Recipe），即可合成并自动装备该武器。

![Crafting Grid GIF](Docs/Images/Crafting.gif)
*(在此处放置合成动画或截图)*

### 2. 战斗系统 (Combat System)
- **多样的武器**: 支持近战（挥砍、刺击）和远程（发射投射物）武器。
- **伤害克制**: 不同的武器拥有不同的伤害类型 (`Blunt`, `Slash`, `Pierce`)。
    - **弱点机制**: 针对敌人的弱点使用对应类型的武器，可造成 **2倍伤害**。

### 3. Buff 系统
在探险过程中，玩家可以获得多种增益 Buff：
- ❤️ **Max Health**: 增加生命值上限。
- ⚡ **Move Speed**: 提升移动速度。
- ⚔️ **Attack Damage**: 提升攻击力。
- 🎲 **Dot Consumption**: **减半** Dot 的消耗概率（放置 Dot 时有 50% 几率免费）。

![Gameplay Screenshot](Docs/Images/Gameplay.png)
*(在此处放置战斗截图)*

## 🕹️ 操作说明 (Controls)

| 按键 | 功能 |
| --- | --- |
| **W A S D** | 移动角色 |
| **鼠标移动** | 瞄准 |
| **鼠标左键** | 攻击 |
| **Tab** | 打开/关闭 武器组装面板 |

## 👾 敌人 (Enemies)

- **Robot**: 基础近战敌人，有强力的拳击。
- **Drone**: 远程射击敌人，会保持距离并尝试风筝玩家。
- **Tank**: 高防御、高质量的坦克型敌人。

## 🛠️ 项目结构

- **GridManager**: 处理核心的 Dot 放置与图案匹配逻辑。
- **PlayerController**: 玩家控制器，处理移动、动画和武器使用。
- **PlayerBuffs**: 统一管理玩家的所有 Buff 状态。
- **EnemyBase / EnemyAI**: 敌人 AI 逻辑框架。
- **DungeonManager**: 处理关卡生成和管理。
- **WeaponManager**: 处理武器的生成和管理。
- **等等**
---
*Created for the Dot-Theme Game Jam 2026/02.*
