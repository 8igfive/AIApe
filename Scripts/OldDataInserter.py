# -*- coding: utf8 -*-

from utils.Utils import LogLevel
from utils.Poster import post
from utils.Poster import get
from utils.Poster import login 
from utils.Poster import signup 
from utils.Poster import NoAuthorization
import utils.Poster as poster
from utils.Utils import log
from utils.Config import Path
from utils.Utils import pcat
from utils.Poster import fresh_token
import json
import os
import time

tag_table = {
    "C语言":("Lang", "C语言, 一门面向过程的、抽象化的通用程序设计语言，广泛应用于底层开发。"),
    "Java":("Lang", "Java是由Sun Microsystems公司于1995年 月推出的高级程序设计语言。Java可运行于多个平台，如Windows, Mac OS及其他多种UNIX版本的系统。"),
    "Python":("Lang", "Python由荷兰数学和计算机科学研究学会的Guido van Rossum 于1990 年代初设计，作为一门叫做ABC语言的替代品。"),
    "SQL":("Lang", "结构化查询语言（Structured Query Language）简称SQL，是一种特殊目的的编程语言，是一种数据库查询和程序设计语言，用于存取数据以及查询、更新和管理关系数据库系统。"),
    "环境配置" : ("Other", "环境配置相关，包括IDE安装、编译器安装。"),
    "工具使用" : ("Other", "开发工具使用相关，包括IDE使用、编译器使用，包括各种报错。"),
    "标准库" : ("Other", "标准库相关，包括标准库函数使用、理解。"),
    "语句" : ("Other", "语句相关，包括循环、分支等语句。"),
    "关键字" : ("Other", "关键字相关，包括if、while、for等。"),
    "代码" : ("Other", "有源代码的问题。"),
    "Linux" : ("Env", "由Linus Benedict Torvalds开发，并由开源社区维护的类Unix操作系统内核Linux。一般代指使用Linux内核的发行版，如Debian、Ubuntu、RHEL、Centos等。"),
    "Windows" : ("Env", "由微软公司（Microsoft）开发的操作系统，是目前最主流的桌面PC操作系统。"),
    "macOS" : ("Env", "由苹果（Apple）开发的操作系统，常用于苹果的Mac系列电脑。"),
    "Dev C++" : ("Env", "Windows 环境下的一个轻量级 C/C++ 集成开发环境（IDE），是一款自由软件，遵守GPL许可协议分发源代码。目前已停止维护，但仍有社区维护的版本。"),
    "Visual C++" : ("Env", "由微软公司（Microsoft）的免费C++集成开发环境（IDE），可提供编辑C语言。目前微软已停止维护并关闭下载渠道。"),
    "VS Code" : ("Env", "由微软公司（Microsoft）同社区开发的Code-OOS的构建，是一款轻量级集成开发环境（IDE），通过插件来支持多种语言的开发。"),
    "Visual Studio" : ("Env", "由微软公司（Microsoft）开发的重量级集成开发环境（IDE）。"),
    "gcc" : ("Env", "GNU Compiler Collection，GNU编译器套件，是遵循GPL协议的自由软件。支持多种语言与多种目标平台。也是GCC编译C语言所使用的的指令名。"),
    "clang" : ("Env", "是LLVM的C家族语言前端，是遵循BSD协议的自由软件。一般可认为是一个C语言、C++、Objective-C语言的轻量级编译器。"),
    "msvc" : ("Env", "由微软公司（Microsoft）开发的C++编译器，是微软旗下IDE（Visual Studio、Visual C++等）的默认C++编译器。"),
}

tid = None

def init_tids():
    global tid
    tid = get("/api/questions/taglist")

def add_tag(name: str, desc: str, category: str) -> int:
    obj = { "name": name, "desc": desc, "category": category}
    log("添加标签：", name)
    res = post("/api/questions/add_tag", obj)
    if res["status"] != "success":
        log("添加标签：", name, "失败", "[{}] {}".format(res["status"], res["message"]), level=LogLevel.WAR)
        return -1
    else:
        tid = res["tid"]
        log("添加标签：", name, "成功", "tid=", tid)
        return tid

def add_question(title: str, remarks: str, tags: list, question_jwt: str=None) -> int:
    global tid
    if tid is None:
        init_tids()
    obj = { "title": title, "remarks": remarks, "tags": [tid[t] for t in tags if t in tid.keys()] }
    log("添加问题：", title[:10])
    res = post("/api/questions/add_question", obj, jwt=question_jwt)
    if res["status"] != "success":
        log("添加问题：", title[:10], "失败", "[{}] {}".format(res["status"], res["message"]), level=LogLevel.WAR)
        return -1
    else:
        qid = res["qid"]
        log("添加问题：", title[:10], "成功", "qid=", qid)
        return qid

def add_answer(qid: int, content: str, answer_jwt: str=None) -> int:
    obj = { "qid": qid, "content": content }
    log("添加回答：", content[:10])
    res = post("/api/questions/add_answer", obj, jwt=answer_jwt)
    if res["status"] != "success":
        log("添加回答：", content[:10], "失败", "[{}] {}".format(res["status"], res["message"]), level=LogLevel.WAR)
        return -1
    else:
        aid = res["aid"]
        log("添加回答：", content[:10], "成功", "aid=", aid)
        return aid

def try_add_all_tags():
    for k, v in tag_table.items():
        add_tag(k, v[1], v[0])

def try_add_all_questions(question_jwt: str, answer_jwt: str):
    with open(Path.Script_CSDNData, 'r', encoding='utf8') as f:
        questions = json.load(f)
    to_add_file = pcat(Path.Script, "questions_to_add.json")
    to_add = None
    if os.path.exists(to_add_file):
        with open(to_add_file, 'r', encoding='utf8') as f:
            to_add = json.load(f)
    else:
        to_add = list(questions)
    added = []
    try:
        for i, question in enumerate(to_add):
            time.sleep(1)
            if i % 64 == 63:
                question_jwt = fresh_token(question_jwt)
                answer_jwt = fresh_token(answer_jwt)
            q = question["q"]
            a = question["a"]
            qid = add_question(q["title"], q["remarks"], q["tags"], question_jwt)
            if qid < 0:
                continue
            aid = add_answer(qid, a["content"], answer_jwt)
            if aid < 0:
                continue
            added.append(question)
    finally:
        for a in added:
            to_add.remove(a)
        with open(to_add_file, 'w', encoding='utf8') as f:
            json.dump(to_add, f)


def main() -> int:
    log("using protocol:", poster.protocol)
    log("using base_url:", poster.base_url)
    while True:
        command = input("signup, tag, question, or exit:\t")
        try:
            if command == "signup":
                signup()
            elif command == "exit":
                return
            elif command == "tag":
                print("please login with admin account")
                login()
                try_add_all_tags()
            elif command == "question":
                print("please login with question account")
                question_jwt = login()
                print("please login with answer account")
                answer_jwt = login()
                try_add_all_questions(question_jwt, answer_jwt)
            else:
                print("unknow command:", command)
        except KeyboardInterrupt:
            print("")
        except NoAuthorization:
            print("login required!")
        

if __name__ == '__main__':
    try:
        main()
        print("exit with exit command")
    except KeyboardInterrupt:
        print("\nexit with keyboard interrupt")