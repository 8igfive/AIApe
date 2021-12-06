import os
import numpy as np
import const
import threading
from tqdm import tqdm
from sentence_transformers import SentenceTransformer


class EmbeddingModel:
    def __init__(self, model_name, max_sentences_num, device, debug=False):
        self.debug = debug
        self.model_name = model_name
        self.max_sentences_num = max_sentences_num
        self.device = device
        self.model = SentenceTransformer(os.path.join(const.MODELS_PATH, self.model_name), device=device)
        self.lock = threading.Lock()
    '''
    returns: ndarray, dtype=float32
    '''
    def embedding(self, sentences):
        embeddings = None
        sentences = [sentences] if isinstance(sentences, str) else sentences
        range_iter = tqdm(range(0, len(sentences), self.max_sentences_num)) if self.debug else range(0, len(sentences), self.max_sentences_num)
        for i in range_iter:
            part_sentences = sentences[i: i + self.max_sentences_num]
            with self.lock:
                part_embeddings = self.model.encode(sentences=part_sentences, batch_size=len(part_sentences))
            if embeddings is None:
                embeddings = part_embeddings
            else:
                embeddings = np.concatenate((embeddings, part_embeddings), axis=0)
        return embeddings


if __name__ == '__main__':
    em = EmbeddingModel(model_name='distillation-sts_2021-05-28_17-33-59', max_sentences_num=1, device='cuda:{}'.format(0))
    ebds = em.embedding([
        "做个测试",
        "就做个简单的测试",
        "再来个测试"
    ])
    print(type(ebds), ebds.shape)