


from DataManager.SelectTags import select_tags
from typing import Dict, List
from utils.Poster import fresh_token, login, post
from utils.Utils import LogLevel, log, pcat
from utils.Config import Path
import time
import os
import json
from .TagManager import get_remote_tag_name2tid_dict

tid = None

def add_question(title: str, remarks: str, tags: list, question_jwt: str=None) -> int:
    global tid
    if tid is None:
        tid = get_remote_tag_name2tid_dict()
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

def _build_all_questions() -> List[Dict[str, object]]:
    ret = []
    pure_dir = pcat(Path.Scripte_DataManager_Datas, "pure")
    auto_tag_dir = pcat(Path.Scripte_DataManager_Datas, "auto_tag")
    select_dir = pcat(Path.Scripte_DataManager_Datas, "select")
    langs = os.listdir(pure_dir)
    for lang in langs:
        pure_lang = pcat(pure_dir, lang)
        auto_tag_lang = pcat(auto_tag_dir, lang)
        select_lang = pcat(select_dir, lang)
        questions = os.listdir(pure_lang)
        for question in questions:
            with open(pcat(pure_lang, question), 'r', encoding='utf8') as f:
                pure_info = json.load(f)
            # TODO
            if len(pure_info["remarks"]) >= (65535/4):
                continue
            tags = set(pure_info["tags"])
            auto_path = pcat(auto_tag_lang, question)
            if not os.path.exists(auto_path):
                log(f"{auto_path} not exist", LogLevel.WAR)
            else:
                with open(auto_path, 'r', encoding='utf8') as f:
                    auto_tags = json.load(f)
                for auto_tag in auto_tags:
                    tags.add(auto_tag)
            select_path = pcat(select_lang, question)
            if not os.path.exists(select_path):
                # log(f"{select_path} not exist", LogLevel.WAR)
                pass
            else:
                with open(select_path, 'r', encoding='utf8') as f:
                    select_tags = json.load(f)
                for to_add in select_tags["add_tags"]:
                    tags.add(to_add)
                for to_remove in select_tags["delete_tags"]:
                    tags.remove(to_remove)
            pure_info["tags"] = list(tags)
            if "删除" not in tags:
                ret.append(pure_info)
    return ret


def push_all_question_to_remote():
    to_add_file = pcat(Path.Scripte_DataManager_Datas, "questions_to_add.json")
    to_add = None
    if os.path.exists(to_add_file):
        with open(to_add_file, 'r', encoding='utf8') as f:
            to_add = json.load(f)
    else:
        questions = _build_all_questions()
        to_add = list(questions)
    added = []
    
    try:
        print("please login with question account")
        question_jwt = login()
        print("please login with answer account")
        answer_jwt = login()
        for i, question in enumerate(to_add):
            # time.sleep(0.1)
            if i % 64 == 63:
                question_jwt = fresh_token(question_jwt)
                answer_jwt = fresh_token(answer_jwt)
            q = question
            qid = add_question(q["title"], q["remarks"], q["tags"], question_jwt)
            if qid < 0:
                continue
            aid = add_answer(qid, q["answer"], answer_jwt)
            if aid < 0:
                continue
            added.append(question)
    finally:
        for a in added:
            to_add.remove(a)
        with open(to_add_file, 'w', encoding='utf8') as f:
            json.dump(to_add, f, ensure_ascii=False)