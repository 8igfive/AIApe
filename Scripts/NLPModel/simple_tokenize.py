import torch
from transformers import AutoTokenizer, AutoModel
import json
import os
import pickle

QA_DIR = r'E:\Schools\3_2\SE\qa_dataset\qa_dataset\process4Model\modifiedDataset'
PYTHON_DIR = os.path.join(QA_DIR, '8')
BERT_ZH_PATH0 = r'E:\Coding\BERT\Models\hfl-chinese-roberta-wwm-ext'

DUMP_SENTENCES_PATH = os.path.join('.', 'embeddings', 'sentences.pkl')
DUMP_IDX2FILENAME_PATH = os.path.join('.', 'embeddings', 'idx2filename.pkl')
DUMP_EMBEDDINGS_PATH = os.path.join('.', 'embeddings', 'embeddings.pkl')

tokenizer = AutoTokenizer.from_pretrained(BERT_ZH_PATH0)
model = AutoModel.from_pretrained(BERT_ZH_PATH0)


def mean_pooling(model_output, attention_mask):
    token_embeddings = model_output[0]
    input_mask_expanded = attention_mask.unsqueeze(-1).expand(token_embeddings.size()).float()
    sum_embeddings = torch.sum(token_embeddings * input_mask_expanded, 1)
    sum_mask = torch.clamp(input_mask_expanded.sum(1), min=1e-9)
    return sum_embeddings / sum_mask


if __name__ == '__main__':
    qaList = os.listdir(PYTHON_DIR)
    sentences = list()
    idx2FileName = dict()
    for qaFile in qaList:
        with open(os.path.join(PYTHON_DIR, qaFile), 'r', encoding='utf8') as f:
            qaJson = json.load(f)
        if '代码' in qaJson['tag_name']:
            continue
        idx2FileName[len(sentences)] = qaFile
        sentences.append(qaJson['rb_answer_content'])
    encoded_input = tokenizer(sentences, padding=True, truncation=True, max_length=128, return_tensors='pt')
    with torch.no_grad():
        model_output = model(**encoded_input)
    embeddings = mean_pooling(model_output, encoded_input['attention_mask'])
    with open(DUMP_SENTENCES_PATH, 'wb') as d:
        pickle.dump(sentences, d)
    with open(DUMP_IDX2FILENAME_PATH, 'wb') as d:
        pickle.dump(DUMP_IDX2FILENAME_PATH, d)
    with open(DUMP_EMBEDDINGS_PATH, 'wb') as d:
        pickle.dump(embeddings, d)

