import os
import sys
from utils.Utils import log
from utils.Utils import pcat
from utils.Utils import LogLevel
from utils.Config import Path
from utils.Utils import exec
from utils.Utils import StopException

def compile():
    log("编译打包")
    exec("dotnet publish --configuration Release {}".format(Path.AIBot))

def move_build():
    log("移动可执行文件")
    origin_path = pcat(pcat(Path.AIBot_Release, "net5.0"), "publish")
    target_path = "/usr/local/bin/aiape/dotnet"
    if os.path.exists(target_path):
        exec("sudo rm -r {}".format(target_path))
    exec("sudo mv {} {}".format(origin_path, target_path))

def link_service():
    log("链接service文件")
    origin_path = pcat(Path.Script, "kestrel-aiape.service")
    target_path = "/etc/systemd/system/kestrel-aiape.service"
    if os.path.exists(target_path):
        exec("sudo rm {}".format(target_path))
    exec("sudo ln -s {} {}".format(origin_path, target_path))

def start():
    log("启动服务")
    service_path = "/etc/systemd/system/kestrel-aiape.service"
    if not os.path.exists(service_path):
        link_service()
        exec("sudo systemctl enable kestrel-aiape.service")
    exec("sudo systemctl start kestrel-aiape.service", ensure_success=False)


def main() -> int:
    while True:
        command = input("compile, start, or exit:\t")
        try:
            if command == "compile":
                compile()
                move_build()
            elif command == "start":
                start()
            elif command == "exit":
                return
            else:
                print("unknow command:", command)
        except KeyboardInterrupt:
            print("")
        except StopException:
            print("stop unexpect!")
        

if __name__ == '__main__':
    try:
        main()
        print("exit with exit command")
    except KeyboardInterrupt:
        print("\nexit with keyboard interrupt")