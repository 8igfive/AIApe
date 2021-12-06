import json
import os
import re

CURRENT_DIR = os.path.dirname(__file__)
DUMP_DIR = os.path.join(CURRENT_DIR, 'modifiedDataset')
QA_DIR = os.path.join(CURRENT_DIR, 'qa_dataset')

PYTHON_DIR = os.path.join(QA_DIR, '8')
DUMP_PYTHON_DIR = os.path.join(DUMP_DIR, '8')

if __name__ == '__main__':
    qaList = os.listdir(PYTHON_DIR)
    removeBlank = re.compile(r'\s+')
    maxAnswerLength = 0
    maxALFile = ''
    notCodeFileCount = 0
    for qa in qaList:
        with open(os.path.join(PYTHON_DIR, qa), 'r', encoding='utf8') as f:
            qaJson = json.load(f)
        qaDump = dict()
        qaDump['question_title'] = removeBlank.sub(' ', qaJson['question_title'])
        qaDump['question_content'] = removeBlank.sub(' ', qaJson['question_content'])
        qaDump['answer_content'] = removeBlank.sub(' ', qaJson['answer_content'])
        qaDump['rb_question_title'] = removeBlank.sub(' ', qaJson['question_title'])
        qaDump['rb_question_content'] = removeBlank.sub(' ', qaJson['question_content'])
        qaDump['rb_answer_content'] = removeBlank.sub(' ', qaJson['answer_content'])
        qaDump['tag_name'] = list()
        enCharCount = 0
        for ch in qaDump['rb_question_content']:
            if ord(ch) >= 97 and ord(ch) <= 122 or ord(ch) >= 65 and ord(ch) <= 90 or ord(ch) >= 48 and ord(ch) <= 57:
                enCharCount += 1
        if enCharCount / len(qaDump['rb_question_content']) >= 0.6: # TODO: 先只检查问题内容
            qaDump['tag_name'].append('代码')
        else:
            notCodeFileCount += 1
            if len(qaDump['rb_answer_content']) > maxAnswerLength:
                maxALFile = qa
            maxAnswerLength = len(qaDump['rb_answer_content'])
        with open(os.path.join(DUMP_PYTHON_DIR, qa), 'w', encoding='utf8') as f:
            json.dump(qaDump, f, indent=4, ensure_ascii=False)
    print('notCodeFileCount: {}\nmaxAnswerLength: {}\nmaxALFile: {}\n'.format(notCodeFileCount, maxAnswerLength, maxALFile))  
            
            
