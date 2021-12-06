# Scrum Meeting Beta 3

**日期：**2021年5月29日
**会议主要内容概述：**汇报工作

## 一、进度情况

| 组员   | 负责 | 两日内已完成的工作                                           | 后两日计划完成的工作                                         | 工作中遇到的困难                                             |
| :----- | :--- | :----------------------------------------------------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 李明昕 | 后端 | 完成了模型蒸馏，模型体积缩小到原先的六分之一，但性能相当<br />使用Flask框架将模型包装成了微服务 | 搜索功能实现<br />验证搜索性能<br />着手完善机器人流程       | 模型蒸馏过程中遇到数据集缺少问题，后来找了一些标准数据集作为补充<br />第一次接触Flask框架，需要一点学习时间 |
| 邓新宇 | 后端 | [Task42](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/42),[Task43](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/43),[Task44](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/44)在服务层添加代码静态分析功能 | [Task80](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/80)热榜相关的功能<br />[Task50](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/50)实现问题搜索功能 | 热度的计算算法设计花费了一些时间。热度的更新需要大量计算，所以花费了较多时间设计了后台更新的机制<br />后台更新需要DbContext的支持，但是通过IoC容器注入的DbContext会被自动销毁，无法长期留存，需要另外的DbContext支持，花费了较多时间 |
| 董俊杰 | 前端 | [Task62](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/62) 聊天页面的移动端适配<br />[Task72](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/72) 聊天页面的移动端适配 | [Task73](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/73) 浏览问题页面的移动端适配 | iphone浏览器没有控制台                                       |
| 黄思为 | 前端 | 完成搜索界面的框架设计<br />优化路由切换时的页面缓存功能     | 完成代码静态分析界面<br />提问界面与后端交互                 | 无                                                           |
| 黎昊轩 | PM   | [Task77](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/77) 完成问题列表界面的细节优化 | [Task82](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/82) 个人主页框架设计<br />[Task83](https://gitlab.buaaoo.top/2021_alige_homeworks/group_projects/dang_qi_shuang_jiang/AIApe/issues/83) 用户管理页面框架设计 | 页面设计完成后，仍存在不恰当的地方，讨论和修改花费了不少时间 |



## 二、燃尽图

![5.29燃尽图](..\images\Scrum Meetings\5.29燃尽图.png)



## 三、会议记录

前后端同学分别汇报了工作；

目前的开发阶段，前端和后端独立进行。后端与前端之间交流及时。前端开发之间、后端开发之间交流频繁，并且时刻针对出现的问题进行及时的解决。一些bug并没有及时反馈到issue中。

在前端的开发过程中，发现遗漏了部分页面的设计。







