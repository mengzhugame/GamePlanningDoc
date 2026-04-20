1.找到unity 安装目录 下的ADB 命令行，C:\Program Files\Tuanjie\Hub\Editor\2022.3.61t1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools
2.再这个目录上运行 cmd命令行
3.再命令行输入 adb devices ，检查设备是否连接正常。（安卓手机需要开启开发者模式）
4.测试unity的端口是否正常 adb forward tcp:34999
5.输入连接命令 adb forward tcp:34999 localabstract:Unity-com.DefaultCompany.LightVSDecay