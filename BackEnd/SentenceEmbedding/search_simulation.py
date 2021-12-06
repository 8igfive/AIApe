import const
from EmbeddingModel import EmbeddingModel
from sentence_transformers import util
import os
import json
import tqdm
import torch
import pickle
import argparse
import numpy as np

QA_EMBED_CACHE_NAME = 'qaembeds_numpy.pkl'
QA_TITLE_CACHE_NAME = 'qapaths_list.pkl'

QA_NAME = 'qa_dataset'

class QASet:
    def __init__(self, model):
        self.model = model
        if QA_EMBED_CACHE_NAME in os.listdir(const.CACHE_PATH):
            with open(os.path.join(const.CACHE_PATH, QA_EMBED_CACHE_NAME), 'rb') as f:
                self.qa_embeds = pickle.load(f)
            with open(os.path.join(const.CACHE_PATH, QA_TITLE_CACHE_NAME), 'rb') as f:
                self.qa_titles = pickle.load(f)
        else:
            self._gen_qa_embeds()
            with open(os.path.join(const.CACHE_PATH, QA_EMBED_CACHE_NAME), 'wb') as f:
                pickle.dump(self.qa_embeds, f)
            with open(os.path.join(const.CACHE_PATH, QA_TITLE_CACHE_NAME), 'wb') as f:
                pickle.dump(self.qa_titles, f)
        print('Load qa_dataset done.')
    
    def _gen_qa_embeds(self):
        if self.model is None:
            print('model has not been initialized.')
            self.qa_embeds = np.array(list())
            self.qa_titles = list()
            return
        qa_dirs = os.listdir(os.path.join(const.CORPUS_PATH, QA_NAME))
        self.qa_titles = list()
        for qa_dir in qa_dirs:
            qa_pairs = os.listdir(os.path.join(const.CORPUS_PATH, QA_NAME, qa_dir))
            for qa_pair in qa_pairs:
                with open(os.path.join(const.CORPUS_PATH, QA_NAME, qa_dir, qa_pair), 'r', encoding='utf8') as f:
                    qa_json = json.load(f)
                self.qa_titles.append(qa_json['question_title'])
        self.qa_embeds = self.model.embedding(self.qa_titles)

class SearchSimulator:
    def __init__(self, dataset, topk, model):
        self.dataset = dataset
        self.topk = topk
        self.model = model
      
    def search(self, sentence):
        embedding = self.model.embedding([sentence])
        cos_scores = util.pytorch_cos_sim(embedding, self.dataset.qa_embeds)[0]
        top_results = torch.topk(cos_scores, k=self.topk)
        ret = list()
        for score, idx in zip(top_results[0], top_results[1]):
            ret.append((score, self.dataset.qa_titles[idx]))
        return ret

def main():
    parser = argparse.ArgumentParser(description='A simulator for searching.')
    parser.add_argument('-mn', '--model_name', type=str, default='distillation-sts_2021-05-28_17-33-59', help='model name')
    parser.add_argument('-msn', '--max_sentences_num', type=int, default=16, help='max sentences number')
    parser.add_argument('-d', '--device', type=str, default='cuda:{}'.format(0), help='device to run the model')
    parser.add_argument('-rg', '--regenerate', action='store_true', help='whether to regenerate the qa_embeds or not')
    parser.add_argument('-ss', '--start_simulation', action='store_true', help='whether to start simulation or not')
    parser.add_argument('-tk', '--topk', type=int, default=10, help='number to show relavent question')

    args = parser.parse_args()

    model = EmbeddingModel(args.model_name, args.max_sentences_num, args.device, debug=True)

    if args.regenerate:
        if QA_EMBED_CACHE_NAME in os.listdir(const.CACHE_PATH):
            os.remove(os.path.join(const.CACHE_PATH, QA_EMBED_CACHE_NAME))
        if QA_TITLE_CACHE_NAME in os.listdir(const.CACHE_PATH):
            os.remove(os.path.join(const.CACHE_PATH, QA_TITLE_CACHE_NAME))
    
    qaSet = QASet(model)

    if args.start_simulation:
        simulator = SearchSimulator(qaSet, args.topk, model)
        while True:
            sentence = input('your question: ')
            if sentence == 'e' or sentence == 'exit':
                break
            result = simulator.search(sentence)
            print("retrieval result is :")
            for rank, record in enumerate(result):
                print('\t{}. score: {}, question title: {}'.format(rank + 1, record[0], record[1]))
                

if __name__ == '__main__':
    main()