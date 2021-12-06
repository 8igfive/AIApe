# Scrum Meeting Beta 4

**日期：**2021年6月3日
**会议主要内容概述：**汇报工作；讨论遇到的困难，制定后续计划

## 一、进度情况

| 组员   | 负责 | 两日内已完成的工作                                           | 后两日计划完成的工作                                         | 工作中遇到的困难                                        |
| :----- | :--- | :----------------------------------------------------------- | :----------------------------------------------------------- | :------------------------------------------------------ |
| 李明昕 | 后端 | [Task49](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/49)扩展了句子特征提取微服务的功能，将句子检索部分移至该微服务（之前想法是放在ASP.NET中完成） | 扩展编程语言相关的问答集<br />添加日常问答集                 | 无困难                                                  |
| 邓新宇 | 后端 | [Task51](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/51)，[Task52](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/52)  机器人实现 | [Task53](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/53) 机器人非NLP部分实现<br />[Task91](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/91)  修改/api/user/answer接口 | 机器人转移状态比较复杂，给设计和实现上都带来了麻烦      |
| 董俊杰 | 前端 | [Task74](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/74) 移动端问题详情<br />[Task75](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/75) 移动端代码分析页面 | [Task86](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/86)移动端个人信息 | npm不知道为什么报了一些奇奇怪怪的错误，具体请见会议记录 |
| 黄思为 | 前端 | [Task61](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/61)登录注册界面优化<br />修复bug:选择标签后返回, 无法无限滚动加载 | [Task84](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/84)用户个人页面内容补充<br />[Task93](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/93)问题列表优化<br />[Task94](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/94)聊天页面去除多余的prompt | 暂无                                                    |
| 黎昊轩 | PM   | [Task78](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/78)问题详情页面细节优化 | [Task63](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/61)优化聊天界面 | 暂无                                                    |



## 二、燃尽图

![6.3燃尽图](D:\BUAA\2021软工\团队项目\proj\Documents\images\Scrum Meetings\6.3燃尽图.png)



## 三、会议记录

本次会议时间较长，主要对于机器人流程、前端用户交互功能和一些细节问题进行了讨论。

董俊杰提出的奇奇怪怪的报错：
![6.3会议记录1](D:\BUAA\2021软工\团队项目\proj\Documents\images\Scrum Meetings\6.3会议记录1.png)



会议讨论出机器人的最新状态图：
![6.3会议记录2](D:\BUAA\2021软工\团队项目\proj\Documents\images\Scrum Meetings\6.3会议记录2.png)

在此基础上，还将新增一个`reset`状态，当用户输入某些关键词，或者用户输入一些捣乱的输入，就会进入reset状态问用户是否重置，如果选是，就会清空状态回到welcome，如果选否，就会恢复到之前的状态。

会议中，前后端进行了大量的对接工作，统一了对机器人设计的思想，整体讨论了交互功能和对应的优化策略。讨论了后续需要进行和补充的工作，已经补充在了[issue](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues)中。

此外，会议还提出了一些不太好解决的问题：

- vue自带的ScrollTop速度太快，无法控制
- 问题预览界面渲染方式

