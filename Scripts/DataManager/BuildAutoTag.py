import json
import os
from utils.Utils import LogLevel, ensure_and_clean_up_dir, log, pcat
from utils.Config import Path, relative_path
from utils.Poster import login, post


def _produce_question(src: str, dst: str):
    with open(src, 'r', encoding='utf8') as f:
        pure = json.load(f)
    res = post("/api/questions/auto_tag", {"title" : pure["title"], "remarks": pure["remarks"]})
    if res["status"] != "success":
        message = res["message"]
        log(f"get auto-tag for {src} fail with message: {message}", LogLevel.WAR)
        return
    tags = res["tags"]
    tag_names = list(tags)
    
    log(f"{relative_path(src)} ---{tag_names}---> {relative_path(dst)}")
    with open(dst, 'w', encoding='utf8') as f:
        json.dump(tag_names, f, ensure_ascii=False)

def generate_all_auto_tags():
    auto_tag_dir = pcat(Path.Scripte_DataManager_Datas, "auto_tag")
    pure_dir = pcat(Path.Scripte_DataManager_Datas, "pure")
    ensure_and_clean_up_dir(auto_tag_dir)
    langs = os.listdir(pure_dir)
    for lang in langs:
        dst_lang_dir = pcat(auto_tag_dir, lang)
        src_lang_dir = pcat(pure_dir, lang)
        ensure_and_clean_up_dir(dst_lang_dir)
        questions = os.listdir(src_lang_dir)
        for question in questions:
            src_path = pcat(src_lang_dir, question)
            dst_path = pcat(dst_lang_dir, question)
            _produce_question(src_path, dst_path)
