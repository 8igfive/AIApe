from utils.Utils import ensure_and_clean_up_dir, log, pcat
from typing import List
from utils.Config import Path, relative_path
import json
import os

class _QuestionAnswerInfo:
    def __init__(self, title:str, remarks:str, answer:str, tags: List[str]):
        self.title = title
        self.remarks = remarks
        self.answer = answer
        self.tags = tags
    
    def to_dict(self) -> dict:
        return {"title" : self.title, "remarks" : self.remarks, "answer" : self.answer,  "tags" : self.tags}

def read_from_csdn(file_path: str, lang_tag: str) -> _QuestionAnswerInfo:
    with open(file_path, 'r', encoding='utf8') as f:
        content = json.load(f)
    title = content["question_title"]
    remarks = content["question_content"]
    answer = content["answer_content"]
    tags = [lang_tag]
    return _QuestionAnswerInfo(title, remarks, answer, tags)

def read_from_crawler(file_path: str, lang_tag: str) -> _QuestionAnswerInfo:
    with open(file_path, 'r', encoding='utf8') as f:
        content = json.load(f)
    title = content["question_title"]
    remarks = content["question_content"]
    answer = content["answer_content"]
    tags = set(content["tags"])
    tags.add(lang_tag)
    return _QuestionAnswerInfo(title, remarks, answer, list(tags))



def _produce_lang(csdn: str, crawler: str, to: str, lang_name: str):
    count = 0
    ensure_and_clean_up_dir(to)
    csdn_jsons = os.listdir(csdn)
    for csdn_json in csdn_jsons:
        src = pcat(csdn, csdn_json)
        dst = pcat(to, str(count)) + ".json"
        count += 1
        log(f"{src} -> {dst}")
        info = read_from_csdn(src, lang_name)
        with open(dst, 'w', encoding='utf8') as f:
            json.dump(info.to_dict(), f, ensure_ascii=False)

    crawler_jsons = os.listdir(crawler)
    for crawler_json in crawler_jsons:
        src = pcat(crawler, crawler_json)
        dst = pcat(to, str(count)) + ".json"
        count += 1
        log(f"{relative_path(src)} -> {relative_path(dst)}")
        info = read_from_crawler(src, lang_name)
        with open(dst, 'w', encoding='utf8') as f:
            json.dump(info.to_dict(), f, ensure_ascii=False)

def raw_to_pure():
    datas = Path.Scripte_DataManager_Datas
    raw_path = pcat(datas, "raw")
    csdn_path = pcat(raw_path, "from_csdn")
    crawler_path = pcat(raw_path, "from_crawler")
    pure_path = pcat(datas, "pure")
    _produce_lang(pcat(csdn_path, "95"), pcat(crawler_path, "C"), pcat(pure_path, "C"), "C语言")
    _produce_lang(pcat(csdn_path, "8"), pcat(crawler_path, "Python"), pcat(pure_path, "Python"), "Python")
    _produce_lang(pcat(csdn_path, "13"), pcat(crawler_path, "Java"), pcat(pure_path, "Java"), "Java")
    _produce_lang(pcat(csdn_path, "411"), pcat(crawler_path, "SQL"), pcat(pure_path, "SQL"), "SQL")



if __name__ == '__main__':
    info = _QuestionAnswerInfo("title", "remarks", "answer", ["tag1", "tag2"])
    print(json.dumps(info.to_dict()))