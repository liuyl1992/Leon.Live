# 说明
## 文件夹划分含义说明：

Website类型的程序集是唯一入口，例如 Leon.Live.API 或者Leon.Live.GRPC。

其他Leon.Live.Plugin前缀的文件夹都是存放指定业务相关的所有代码集合，原则上各个Leon.Live.Plugin.* 互相无引用关系（业务界限比较清晰利于长期维护）。

内部以代码功能划分层次结构，例如

Controller文件夹：api控制器

Model文件夹：输入输出和其他辅助实体

Entity文件夹：数据库实体层对应表(依赖较多可单独程序集)

Service文件夹：业务逻辑服务层

Repository文件夹：数据仓储层(微服务不推荐再分此层)
