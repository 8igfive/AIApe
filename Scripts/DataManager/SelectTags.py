from typing import List, Dict
from utils.Utils import log, pcat
from .MarkTool.app import start
from utils.Config import Path
import os
import json

def _get_lang_from_cosnole(languages: List[str])-> str:
    while True:
        lang = input("select a language in " + ", ".join(languages) + ":\t")
        if lang in languages:
            return lang
        print("unknown language")

def _get_categore2namelist_dict()->Dict[str, List[str]]:
    ret = {}
    with open(pcat(Path.Scripte_DataManager_Datas, "tags.json"), "r", encoding='utf8') as f:
        tag_infos = json.load(f)
    for tag_name, tag_info in tag_infos.items():
        category = tag_info["category"]
        if category in ret:
            ret[category].append(tag_name)
        else:
            ret[category] = [tag_name]
    ret["Lang"].append("删除")
    return ret
        

def select_tags():
    languages = os.listdir(pcat(Path.Scripte_DataManager_Datas, "pure"))
    if len(languages) == 0:
        print("please call 'raw2pure' before calling this")
    tags_full_dict = _get_categore2namelist_dict()
    log(f"category_dict:\n{json.dumps(tags_full_dict, indent=4, ensure_ascii=False)}")
    tags_simple_dict = {}
    for k, v in tags_full_dict.items():
        tags_simple_dict[k] = list(v)
    lang = _get_lang_from_cosnole(languages)
    start(lang=lang, tags=tags_simple_dict)
