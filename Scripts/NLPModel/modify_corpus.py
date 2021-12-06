import os
import re

CURRENT_DIR = os.path.dirname(__file__)

CORPUS_DIR = r'E:\Coding\BERT\Corpus'
CORPUS_DUMP_DIR = os.path.join(CURRENT_DIR, 'corpus')

NLI_DIR = os.path.join(CORPUS_DIR, 'xnli')
NLI_DUMP_DIR = os.path.join(CORPUS_DUMP_DIR, 'modified_xnli')


def modify_xnli():
    file_lists = ['dev.tsv', 'test.tsv', 'train.tsv']
    for file in file_lists:
        with open(os.path.join(NLI_DIR, file), 'r', encoding='utf8') as fIn:
            modified_content = re.sub(r' +', '', fIn.read())
        with open(os.path.join(NLI_DUMP_DIR, file), 'w', encoding='utf8') as fOut:
            fOut.write(modified_content)


if __name__ == '__main__':
    modify_xnli()
