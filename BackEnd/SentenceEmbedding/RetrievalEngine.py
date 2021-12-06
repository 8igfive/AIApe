import const
import torch
from sentence_transformers import util

class RetrievalEngine:
    def __init__(self, model):
        self.model = model
    
    def retrieve(self, sentence, datum, number):
        if number > len(datum):
            return False
        cosine_score = util.pytorch_cos_sim(sentence, datum)[0]
        top_results = torch.topk(cosine_score, k=number)
        ret = list()
        for score, idx in zip(top_results[0], top_results[1]):
            ret.append((score, idx))
        return ret
