from typing import List
from utils.Utils import LogLevel, log, pcat
from utils.Config import Path
from utils.Poster import fresh_token, login, post
import os
import json
import time

def add_natrual(questions: List[str], answers: List[str], admin_jwt: str)->bool:
    obj = { "questions": questions, "answers": answers}
    log("添加自然问答：", questions[0][:10])
    res = post("/api/bot/add_natrual", obj, jwt=admin_jwt)
    if res["status"] != "success":
        log("添加自然问答：", questions[0][:10], "失败", "[{}] {}".format(res["status"], res["message"]), level=LogLevel.WAR)
        return False
    else:
        log("添加自然问答：", questions[0][:10], "成功")
        return True


def _build_all_natruals():
    ret = []
    natrual_dir = pcat(pcat(pcat(Path.Scripte_DataManager_Datas, "raw"), "from_crawler"), "Natrual")
    files = [pcat(natrual_dir, f) for f in os.listdir(natrual_dir)]
    for file in files:
        with open(file, 'r', encoding='utf8') as f:
            data = json.load(f)
        ret.append({"questions" : data["questions"], "answers" : data["answer"]})
    return ret


def push_all_natrual_to_remote():
    to_add_file = pcat(Path.Scripte_DataManager_Datas, "natruals_to_add.json")
    to_add = None
    if os.path.exists(to_add_file):
        with open(to_add_file, 'r', encoding='utf8') as f:
            to_add = json.load(f)
    else:
        natruals = _build_all_natruals()
        to_add = list(natruals)
    added = []

    try:
        print("please login with admin account")
        admin_jwt = login()
        for i, natrual in enumerate(to_add):
            # time.sleep(0.1)
            if i % 64 == 63:
                admin_jwt = fresh_token(admin_jwt)
            add_natrual(natrual["questions"], natrual["answers"], admin_jwt)
            added.append(natrual)
            
    finally:
        for a in added:
            to_add.remove(a)
        with open(to_add_file, 'w', encoding='utf8') as f:
            json.dump(to_add, f, ensure_ascii=False)