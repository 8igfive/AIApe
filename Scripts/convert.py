import os
import json
import pickle

'''
问答数据集需要放在同级目录下
'''

QUESTIONS_PATH = r'./'
CQ_PATH = QUESTIONS_PATH + r'95/'
CACHE_NAME = 'cache'
QUESTIONS_LIST_DUMP_NAME = 'question_list_dump'
DUMP_DIR = 'after_process/'

def main():
    questions_list = None
    if QUESTIONS_LIST_DUMP_NAME in os.listdir('.'):
        with open('./{}'.format(QUESTIONS_LIST_DUMP_NAME), 'rb') as f:
            questions_list = pickle.load(f)
    else:
        questions_list = os.listdir('{}/{}'.format(
            QUESTIONS_PATH,
            CQ_PATH
        ))
    for i, file_name in enumerate(questions_list):
        if i % 100 == 0:
            print(i)
        with open('{}/{}/{}'.format(
            QUESTIONS_PATH,
            CQ_PATH,
            file_name
        ), 'r', encoding='utf8') as f:
            content = json.load(f)
        dump = dict()
        dump['q'] = dict()
        dump['q']['title'] = content['question_title']
        dump['q']['remarks'] = content['question_content']
        if isinstance(content['tag_name'], str):
            dump['q']['tags'] = list()
        else:
            tags = content['tag_name']
            if 'UU'  in tags:
                continue
            if '关键词' in tags:
                tags.remove('关键词')
                tags.append('关键字')
            dump['q']['tags'] = tags
        dump['a'] = dict()
        dump['a']['content'] = content['answer_content']
        with open('{}/{}/{}'.format(
            DUMP_DIR,
            CQ_PATH,
            '{}.json'.format(i)
        ), 'w', encoding='utf8') as f:
            json.dump(dump, f, indent=4, ensure_ascii=False)
    

if __name__ == '__main__':
    if '95' not in os.listdir('.'):
        print('需要将数据集放在统计目录下')
    else:
        main()