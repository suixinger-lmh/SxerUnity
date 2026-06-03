# SxerAudioSystem

#### 介绍
Sxer.AudioSystem 音频系统


#### 软件架构
AudioManager：音频管理接口
AudioSystemBase：低层音频系统基类，提供实际的音频播放功能

IAudioBus：音频总线--控制一类音频的管理
IAudioPlayer：音频实体对象，每个对象提供一个音频播放

粗略实现了FMOD：
直接播放

#### TODO
FMOD播放完善
unity的音频版本实现


#### 使用说明
调用AudioManager的注册方法，把实现好的音频系统注册进去。
通过接口操控音频播放停止等。



