from DataManager.QuestionPusher import push_all_question_to_remote
from DataManager.SelectTags import select_tags
from DataManager.BuildAutoTag import generate_all_auto_tags
from DataManager.TagManager import push_all_tags_to_remote
from DataManager.NatrualPusher import push_all_natrual_to_remote
import utils.Poster
from DataManager.Raw2Pure import raw_to_pure
from utils.Utils import log, pcat
from utils.Poster import login
from utils.Poster import signup
from utils.Utils import enable_log_file
from utils.Config import Path

def help():
    msg = """
首先，将DataManager/Datas文件夹整理成如下形状：
/Datas
    ├─auto_tag
    ├─pure
    ├─raw
    │  ├─from_crawler
    │  │  ├─C
    │  │  ├─Java
    │  │  ├─Natrual
    │  │  ├─Other
    │  │  ├─Python
    │  │  └─SQL
    │  └─from_csdn
    │      ├─13
    │      ├─411
    │      ├─8
    │      └─95
    ├─log.txt
    ├─tags.json
    └─select
其中日志将被记录在log.txt中；
修改/utils/Post.py中22行的下标选择合适的远端；
调用raw2pure将原始信息中的无用信息进行过滤，保存于pure文件夹下；
调用tags将同步推送至远端；
调用autotag通过远端判断，为所有问题添加一个自动生成的标签，其信息将记录于auto_tag文件夹下；
调用select进行人工添改标签；
调用questions将本地问题同步至远端；
调用natrual将本地自然问答同步至远端；
    """
    print(msg)

def main():
    apps = {
        "signup": signup,
        "login": login,
        "raw2pure": raw_to_pure,
        "tags": push_all_tags_to_remote,
        "autotag": generate_all_auto_tags,
        "select": select_tags,
        "questions" : push_all_question_to_remote,
        "natrual": push_all_natrual_to_remote,
        "help" : help
    }
    prompt = ", ".join(apps)
    try:
        while True:
            command = input(prompt + " or exit:\t")
            if command in apps:
                try:
                    apps[command]()
                except KeyboardInterrupt:
                    pass
            elif command in ["exit", "quit", "q", "x"]:
                break
            else:
                print("unknown command")
    except KeyboardInterrupt:
        log("exit because ^C")


if __name__ == '__main__':
    with enable_log_file(pcat(Path.Scripte_DataManager_Datas, "log.txt")):
        main()
