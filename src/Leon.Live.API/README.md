# 说明

视频辅助服务
支持多个视频服务解决方案

## ZLMediaKit
Github:[ZLMediaKit](https://github.com/ZLMediaKit/ZLMediaKit)
ZLMediaKit提供API方式，将视频源地址发给ZLMediaKit，由ZLMediaKit主动进行拉流后再进行视频的分发，分发的视频流支持主流绝大部分协议，例如rtmp;flv;hls等

## SRS
Github:[SRS](https://github.com/ossrs/srs/)
SRS主要以主动通过ffmpeg或OBS进行推流到SRS服务器，由SRS服务器负责视频流的分发，支持rtmp;flv;hls

architecture diagram：

![](../../docs/mediaArchite.png)

architecture flow:

![](../../docs/mediaflow.png)

## 文件夹划分规则：

除了Startup和Protos文件夹外，其他都是以一个业务一个文件夹划分

每个`大业务模块`一个文件夹

在业务模块下每个`功能层`一个文件夹

```
├── Examples(大业务模块)
│       ├── Controller （控制器层） 功能层
│       ├── Model（输入输出model实体）功能层
│       ├── Service（业务服务层）功能层
│       ├── Mapper （实体映射层）功能层
│       └── Proxy（远程请求层）功能层
│        ...
│
├── GlobalizationDemo(大业务模块)
│       ├── Controller （控制器层） 功能层
│       ├── Model（输入输出model实体）功能层
│       ├── Service（业务服务层）功能层
│       ├── Mapper （实体映射层）功能层
│       └── Proxy（远程请求层）功能层
│        ...
├──TimeZoneDemo(大业务模块)
│       ├── ...
│ ...

```

## 其他文件夹说明

### Startup 文件夹
此处存放继承了`INetProStartup`接口的依赖注入的startuo实现类，执行顺序依靠实现类的Order属性控制，可在控制台和自动生成的startup.json文件中查看执行顺序并支持自定义，Order值越大越靠后执行

## Reference

> https://www.cnblogs.com/TSINGSEE/p/14037429.html
