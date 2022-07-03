
# Leon.Live 简介
Leon.Live是一个跨平台Webapi服务,基于开源组件[LeonKou/NetPro](https://github.com/LeonKou/NetPro)，由netproapi脚手架生成，运行时依赖于.Net6.0+。



摄像头发现协议使用[Mictlanix.DotNet.OnvifClient](Mictlanix.DotNet.OnvifClient)


---
# FFmpeg 
FFmpeg全名是Fast Forward MPEG(Moving Picture Experts Group)是一个集成了各种编解码器的库；从视频采集、视频编码到视频传输（包括RTP、RTCP、RTMP、RTSP等等协议）都可以直接使用FFMPEG来完成，更重要的一点FFMPEG是跨平台的，Windows、Linux、Aandroid、IOS这些主流系统通吃

## 安装

- 下载[ffmpeg](https://github.com/BtbN/FFmpeg-Builds/releases)
- 设置环境变量
既配置环境变量指定bin目录
```shell
$env:Path += ";E:\ffmpeg-gpl-shared\bin" #(windows)
```
```shell
export PATH="$PATH:/ffmpeg-gpl-shared/bin" #(linux)
```

## 版本
FFMPEG [https://github.com/BtbN/FFmpeg-Builds/releases](https://github.com/BtbN/FFmpeg-Builds/releases)分为3个版本：Static，Shared，Dev。前两个版本可以直接在命令行中使用，他们的区别在于：
- Static(静态库版本): 里面只有3个应用程序：ffmpeg.exe，ffplay.exe，ffprobe.exe，每个exe的体积都很大，相关的Dll已经被编译到exe里面去了。作为工具而言此版本就可以满足我们的需求；
- Shared（动态库版本）:里面除了3个应用程序：ffmpeg.exe，ffplay.exe，ffprobe.exe之外，还有一些Dll，比如说avcodec-54.dll之类的。Shared里面的exe体积很小，他们在运行的时候，到相应的Dll中调用功能。程序运行过程必须依赖于提供的dll文件；
- Dev（开发者版本）:是用于开发的，里面包含了库文件xxx.lib以及头文件xxx.h，这个版本不包含exe文件。dev版本中include文件夹内文件用途

```


libavcodec：用于各种类型声音/图像编解码；
libavdevice：用于音视频数据采集和渲染等功能的设备相关;
libavfilter：包含多媒体处理常用的滤镜功能;
libavformat：包含多种多媒体容器格式的封装、解封装工具;
libavutil：包含一些公共的工具函数；
libpostproc：用于后期效果处理；
libswresample：用于音频重采样和格式转换等功能;
libswscale：用于视频场景比例缩放、色彩映射转换；
```

# RTSP服务器
rtsp服务器通过ffmpeg推流拉流
- RTSP服务器:
GO开发的[rtsp-simple-server](https://github.com/aler9/rtsp-simple-server)，支持[多系统版本](https://github.com/aler9/rtsp-simple-server/releases)
C++ 开发的[srs](https://github.com/ossrs/srs)，Bee 版本是一个简单高效的实时视频服务器，支持RTMP/WebRTC/HLS/HTTP-FLV/SRT。

-  RTSP C# nuget
[RtspClientSharp](https://github.com/BogdanovKirill/RtspClientSharp)

- rtsp格式
默认rtsp://192.168.8.100/Streaming/Channels/101?transportmode=unicast&profile=Profile_1 （海康）
如需认证 rtsp://账户名:账户密码@192.168.8.100
## 安装
- 安装ffmpeg
- 下载[rtsp-simple-server](https://github.com/aler9/rtsp-simple-server/releases)
- 启动

windows:
```ps
 ./rtsp-simple-server.exe 
```
linux:
```shell
./rtsp-simple-server
```

docker:
需要将ffmpeg打包到镜像中
Dockerfile
```Dockerfile
FROM aler9/rtsp-simple-server AS rtsp
FROM alpine:3.12
RUN apk add --no-cache ffmpeg
COPY --from=rtsp /rtsp-simple-server /
COPY --from=rtsp /rtsp-simple-server.yml / 
ENTRYPOINT [ "/rtsp-simple-server" ]

```
```shell
docker build -t rtsp-server .  # 打包docker镜像
docker login dockerhub.com  #登录
docker tag rtsp-server dockerhub/library/rtsp-server # 打tag
docker push dockerhub/library/rtsp-server # 推送仓库
docker run --rm -d -e RTSP_PROTOCOLS=tcp --restart always -p 8554:8554 -p 1935:1935  -p 8888:8888 library/rtsp-server #运行
docker logs -f --tail 100  rtsp-server # 查看日志

```
资源占用
```
CONTAINER ID   NAME             CPU %     MEM USAGE / LIMIT     MEM %     NET I/O         BLOCK I/O   PIDS
67480587e8e9   wonderful_pike   3.10%     8.141MiB / 7.637GiB   0.10%     713MB / 683MB   0B / 0B     14

# 2个视频流内存占用仅8M CPU使用率仅为3.1%
```

## 使用
1、mp4 通过rtsp推流到RTSP服务器
```shell
ffmpeg -re -stream_loop -1 -i in.mp4 -c copy -f rtsp rtsp://192.168.0.91:8554/mystream
```
- -re  是以流的方式读取
- -stream_loop -1   表示无限循环读取
- -i  就是输入的文件
- -f  格式化输出到哪里

2、MP4通过rtsp推流示例2
```shell
ffmpeg -re -i /home/xx/Documents/in.mp4 -c copy -f rtsp rtsp://192.168.74.130:8554/room1
```
- -re  是以流的方式读取
- -i  就是输入的文件
- -f  格式化输出到哪里
- -c copy 编码器不变

3、rtsp通过rtmp协议推流到RTSP服务器(常用于监控摄像机)
```shell
ffmpeg -i "rtsp://admin:111111@10.16.128.16:66/Streaming/Channels/103?transportmode=unicast&profile=Profile_3" -vcodec copy -acodec copy -f flv -r 11 "rtmp://127.0.0.1:1935/live/livestream" # rtsp 转RTMP ；先启动rtsp-simple-server程序再执行以下命令;rtsp-simple-serverde rtmp端口默认1935
```
响应成功：
```shell
ffmpeg version N-107055-g73302aa193-20220606 Copyright (c) 2000-2022 the FFmpeg developers
  built with gcc 11.2.0 (crosstool-NG 1.24.0.533_681aaef)
  configuration: --prefix=/ffbuild/prefix --pkg-config-flags=--static --pkg-config=pkg-config --cross-prefix=x86_64-w64-mingw32- --arch=x86_64 --target-os=mingw32 --enable-gpl --enable-version3 --disable-debug --enable-shared --disable-static --disable-w32threads --enable-pthreads --enable-iconv --enable-libxml2 --enable-zlib --enable-libfreetype --enable-libfribidi --enable-gmp --enable-lzma --enable-fontconfig --enable-libvorbis --enable-opencl --disable-libpulse --enable-libvmaf --disable-libxcb --disable-xlib --enable-amf --enable-libaom --enable-libaribb24 --enable-avisynth --enable-libdav1d --enable-libdavs2 --disable-libfdk-aac --enable-ffnvcodec --enable-cuda-llvm --enable-frei0r --enable-libgme --enable-libass --enable-libbluray --enable-libjxl --enable-libmp3lame --enable-libopus --enable-librist --enable-libtheora --enable-libvpx --enable-libwebp --enable-lv2 --enable-libmfx --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopenmpt --enable-librav1e --enable-librubberband --enable-schannel --enable-sdl2 --enable-libsoxr --enable-libsrt --enable-libsvtav1 --enable-libtwolame --enable-libuavs3d --disable-libdrm --disable-vaapi --enable-libvidstab --enable-vulkan --enable-libshaderc --enable-libplacebo --enable-libx264 --enable-libx265 --enable-libxavs2 --enable-libxvid --enable-libzimg --enable-libzvbi --extra-cflags=-DLIBTWOLAME_STATIC --extra-cxxflags= --extra-ldflags=-pthread --extra-ldexeflags= --extra-libs=-lgomp --extra-version=20220606
  libavutil      57. 26.100 / 57. 26.100
  libavcodec     59. 33.100 / 59. 33.100
  libavformat    59. 24.100 / 59. 24.100
  libavdevice    59.  6.100 / 59.  6.100
  libavfilter     8. 40.100 /  8. 40.100
  libswscale      6.  6.100 /  6.  6.100
  libswresample   4.  6.100 /  4.  6.100
  libpostproc    56.  5.100 / 56.  5.100
Input #0, rtsp, from 'rtsp://admin:Dswy8866@10.16.37.6:554/Streaming/Channels/103?transportmode=unicast&profile=Profile_3':
  Metadata:
    title           : Media Presentation
  Duration: N/A, start: 0.000000, bitrate: N/A
  Stream #0:0: Video: h264 (Main), yuvj420p(pc, bt709, progressive), 704x576, 10 fps, 25 tbr, 90k tbn
  Stream #0:1: Audio: aac (LC), 16000 Hz, mono, fltp
Output #0, flv, to 'rtmp://127.0.0.1:1935/live':
  Metadata:
    title           : Media Presentation
    encoder         : Lavf59.24.100
  Stream #0:0: Video: h264 (Main) ([7][0][0][0] / 0x0007), yuvj420p(pc, bt709, progressive), 704x576, q=2-31, 10 fps, 25 tbr, 1k tbn
  Stream #0:1: Audio: aac (LC) ([10][0][0][0] / 0x000A), 16000 Hz, mono, fltp
Stream mapping:
  Stream #0:0 -> #0:0 (copy)
  Stream #0:1 -> #0:1 (copy)
Press [q] to stop, [?] for help
[flv @ 0000018fd525e780] Timestamps are unset in a packet for stream 0. This is deprecated and will stop working in the future. Fix your code to set the timestamps properly
[rtsp @ 0000018fd706c040] max delay reached. need to consume packet=1003.2kbits/s speed=0.984x
[rtsp @ 0000018fd706c040] RTP: missed 207 packets
frame= 1793 fps= 10 q=-1.0 00000000000000000000000000000000size=   21966kB time=00:03:01.56 bitrate= 991.1kbits/s speed=1.02x
```

- -i 远程rtsp文件地址
- -r  fps 每秒传输帧数 
- -s  分辨率
- -an 转rtmp后的地址（ffmpeg当rtmp服务器）

响应成功：
```shell
ffmpeg version N-107055-g73302aa193-20220606 Copyright (c) 2000-2022 the FFmpeg developers
  built with gcc 11.2.0 (crosstool-NG 1.24.0.533_681aaef)
  configuration: --prefix=/ffbuild/prefix --pkg-config-flags=--static --pkg-config=pkg-config --cross-prefix=x86_64-w64-mingw32- --arch=x86_64 --target-os=mingw32 --enable-gpl --enable-version3 --disable-debug --enable-shared --disable-static --disable-w32threads --enable-pthreads --enable-iconv --enable-libxml2 --enable-zlib --enable-libfreetype --enable-libfribidi --enable-gmp --enable-lzma --enable-fontconfig --enable-libvorbis --enable-opencl --disable-libpulse --enable-libvmaf --disable-libxcb --disable-xlib --enable-amf --enable-libaom --enable-libaribb24 --enable-avisynth --enable-libdav1d --enable-libdavs2 --disable-libfdk-aac --enable-ffnvcodec --enable-cuda-llvm --enable-frei0r --enable-libgme --enable-libass --enable-libbluray --enable-libjxl --enable-libmp3lame --enable-libopus --enable-librist --enable-libtheora --enable-libvpx --enable-libwebp --enable-lv2 --enable-libmfx --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopenmpt --enable-librav1e --enable-librubberband --enable-schannel --enable-sdl2 --enable-libsoxr --enable-libsrt --enable-libsvtav1 --enable-libtwolame --enable-libuavs3d --disable-libdrm --disable-vaapi --enable-libvidstab --enable-vulkan --enable-libshaderc --enable-libplacebo --enable-libx264 --enable-libx265 --enable-libxavs2 --enable-libxvid --enable-libzimg --enable-libzvbi --extra-cflags=-DLIBTWOLAME_STATIC --extra-cxxflags= --extra-ldflags=-pthread --extra-ldexeflags= --extra-libs=-lgomp --extra-version=20220606
  libavutil      57. 26.100 / 57. 26.100
  libavcodec     59. 33.100 / 59. 33.100
  libavformat    59. 24.100 / 59. 24.100
  libavdevice    59.  6.100 / 59.  6.100
  libavfilter     8. 40.100 /  8. 40.100
  libswscale      6.  6.100 /  6.  6.100
  libswresample   4.  6.100 /  4.  6.100
  libpostproc    56.  5.100 / 56.  5.100
Input #0, rtsp, from 'rtsp://admin:Dswy8866@10.16.37.6:554/Streaming/Channels/103?transportmode=unicast&profile=Profile_3':
  Metadata:
    title           : Media Presentation
  Duration: N/A, start: 0.000000, bitrate: N/A
  Stream #0:0: Video: h264 (Main), yuvj420p(pc, bt709, progressive), 704x576, 10 fps, 25 tbr, 90k tbn
  Stream #0:1: Audio: aac (LC), 16000 Hz, mono, fltp
[hls muxer @ 000001e5d89c4b80] No HTTP method set, hls muxer defaulting to method PUT.
Output #0, hls, to 'http://127.0.0.1:8888/live/test.m3u8':
  Metadata:
    title           : Media Presentation
    encoder         : Lavf59.24.100
  Stream #0:0: Video: h264 (Main), yuvj420p(pc, bt709, progressive), 704x576, q=2-31, 10 fps, 25 tbr, 90k tbn
  Stream #0:1: Audio: aac (LC), 16000 Hz, mono, fltp
Stream mapping:
  Stream #0:0 -> #0:0 (copy)
  Stream #0:1 -> #0:1 (copy)
Press [q] to stop, [?] for help
[hls @ 000001e5d83bddc0] Timestamps are unset in a packet for stream 0. This is deprecated and will stop working in the future. Fix your code to set the timestamps properly
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test0.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test1.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test2.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test3.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test4.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test5.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test6.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test7.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test8.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test9.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test10.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test11.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test12.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test13.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test14.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test15.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test16.ts' for writing
[hls @ 000001e5d83bddc0] Opening 'http://127.0.0.1:8888/live/test17.ts' for writing
```
5、rtsp转rtsp（减少延迟降低IDR帧间隔）
```shell
ffmpeg -i rtsp://original-stream -pix_fmt yuv420p -c:v libx264 -preset ultrafast -b:v 600k -max_muxing_queue_size 1024 -g 30 -f rtsp rtsp://localhost:$RTSP_PORT/compressed
```

## 拉流


rtsp-simple-server 服务支持Rtmp、Rtsp、HLS三种协议拉流

通过Rtmp推流到rtsp-simple-server后，会默认支持以上三种协议拉流

```
[RTSP] listener opened on :8554 (TCP)
[RTMP] listener opened on :1935
[HLS] listener opened on :8888
```

例如推流路径为 /live/livestream
```
 ffmpeg -i "rtsp://username:pwd@172.168.55.77:80/Streaming/Channels/103?transportmode=unicast&profile=Profile_3" -r 5 -c copy -f flv rtmp://rtsp-simple-serverhost/live/livestream
```

客户端HLS拉流

方式一  直接嵌入iframe方式

不需要指定m3u8后缀
```html
<iframe src="http://rtsp-simple-server-ip:8888/mystream" scrolling="no"></iframe>
```

方式二 利用hls库播放m3u8

必须指定m3u8后缀

```html
<video src="http://localhost:8888/mystream/index.m3u8"></video>
```
注意：大多数浏览器不直接支持HLS（Safari除外）;必须使用Javascript库（如hls.js）来加载流。