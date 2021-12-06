


from typing import Dict, Set
from utils.Poster import get, login, post
from utils.Config import Path
from utils.Utils import log, pcat
import json

_cached_local_tag_set = None
_cached_remote_tag_dict = None

def get_local_tag_set() -> Set[str]:
    global _cached_local_tag_set
    if _cached_local_tag_set is None:
        with open(pcat(Path.Scripte_DataManager_Datas, "tags.json"), "r", encoding='utf8') as f:
            tag_infos = json.load(f)
        _cached_local_tag_set = set(tag_infos)
    return _cached_local_tag_set
            
def get_remote_tag_name2tid_dict() -> Dict[str, int]:
    global _cached_remote_tag_dict
    if _cached_remote_tag_dict is None:
        _cached_remote_tag_dict = get("/api/questions/taglist")
    return _cached_remote_tag_dict

def push_all_tags_to_remote():
    print("please login a admin account")
    jwt = login()
    with open(pcat(Path.Scripte_DataManager_Datas, "tags.json"), "r", encoding='utf8') as f:
        tag_infos = json.load(f)
    for tag_name, tag_info in tag_infos.items():
        res = post("/api/questions/add_tag", \
            body = {"name" : tag_name, "desc" : tag_info["desc"], "category": tag_info["category"] }, 
            jwt = jwt)
        if res["status"] == "success":
            tid = res["tid"]
            log(f"add tag {tag_name} success with tid={tid}")
        else:
            status = res["status"]
            message = res["message"]
            log(f"add tag {tag_name} fail ({status}) with message: {message}")

if __name__ == '__main__':
    print(get_local_tag_set())
    print(get_remote_tag_name2tid_dict())
