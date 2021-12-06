import os
import json
import re
import pickle

'''
问答数据集需要放在同级目录下
'''

QUESTIONS_PATH = r'./'
CQ_PATH = QUESTIONS_PATH + r'95/'
CACHE_NAME = 'cache'
QUESTIONS_LIST_DUMP_NAME = 'question_list_dump'

class QA:
    def __init__(self, file_path):
        self.file_path = file_path
        with open(file_path, 'r', encoding='utf8') as f:
            self.content = json.load(f)
        if isinstance(self.content['tag_name'], str):
            self.content['tag_name'] = list()
    
    def show(self):
        print('Qid: {}\nTitle:\n\n{}\n\nRemark:\n\n{}\n\nAnswer:\n\n{}\n\nTag:\n\n{}\n\n'.format(
            self.content['question_id'],
            self.content['question_title'],
            self.content['question_content'],
            self.content['answer_content'],
            self.content['tag_name']
        ))

    def add_tag(self, new_tag):
        self.content['tag_name'].append(new_tag)

    def dump(self, dump_path=None):
        if dump_path == None:
            dump_path = self.file_path
        with open(dump_path, 'w', encoding='utf8') as f:
            json.dump(self.content, f, indent=4, ensure_ascii=False)

class AddTagSystem:
    def __init__(self, type_path):
        self.questions_dir = type_path
        if QUESTIONS_LIST_DUMP_NAME in os.listdir('.'):
            with open('./{}'.format(QUESTIONS_LIST_DUMP_NAME), 'rb') as f:
                self.questions_list = pickle.load(f)
        else:
            self.questions_list = os.listdir(type_path)
            with open('./{}'.format(QUESTIONS_LIST_DUMP_NAME), 'wb') as f:
                pickle.dump(self.questions_list, f)
        self.index = -1
        self.present_qa = None
        self.sep_re = re.compile(r'[\s,]+')
        self.running = True
        print('System starts successfully, type n/next/move/step to show first QA.')
    
    def add_tag(self, new_tags:str):
        tags = self.sep_re.split(new_tags)
        for tag in tags:
            tag = tag.replace('_', ' ')
            self.present_qa.add_tag(tag)
        print('Tags now: {}'.format(self.present_qa.content['tag_name']))


    def next_question(self):
        if self.present_qa != None:
            self.present_qa.dump()
        self.index += 1
        if self.index > len(self.questions_list):
            print('System has finish all questions, ready to exit.')
            self.running = False
        self.present_qa = QA('{}/{}'.format(self.questions_dir, self.questions_list[self.index]))
        self.show_question()

    def show_question(self):
        if self.present_qa != None:
            print('SystemId: {}'.format(self.index))
            self.present_qa.show()
        else:
            print('System hold no QA, type n/next/move/step to start one.')

    def run(self):
        while self.running:
            print('\n{}>>>'.format(self.index), end='')
            cmd = input().strip()
            if cmd == 'n' or cmd == 'next' or cmd == 'move' or cmd == 'step':
                self.next_question()
            elif cmd == 's' or cmd == 'show':
                self.show_question()
            elif cmd == 'q' or cmd == 'quit':
                self.running = False
                index = -1
                if CACHE_NAME in os.listdir('.'):
                    with open('./{}'.format(CACHE_NAME), 'r') as f:
                        index = int(f.read())
                if index < self.index:
                    with open('./{}'.format(CACHE_NAME), 'w') as f:
                        f.write(str(self.index))
            elif cmd == 'c' or cmd == 'continue':
                with open('./{}'.format(CACHE_NAME), 'r') as f:
                    self.index = int(f.read())
                self.index -= 1
                self.next_question()
            elif 'goto' in cmd:
                index = int(re.match(r'goto\s*(\d+)', cmd).group(1))
                self.index = index - 1
                self.next_question()
            elif cmd != '':
                self.add_tag(cmd)
        print('System exit successfully.')

if __name__ == '__main__':
    if '95' not in os.listdir('.'):
        print('需要将数据集放在统计目录下')
    else:
        runner = AddTagSystem(CQ_PATH)
        runner.run()