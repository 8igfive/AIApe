from transformers import AutoTokenizer, AutoModel
from sentence_transformers import SentenceTransformer, util
import torch

BERT_EN_PATH = r'E:\Schools\3rd_1st\MachineLearning\imdb_predict\bert_base_uncased'
BERT_ZH_PATH0 = r'E:\Coding\BERT\Models\hfl-chinese-roberta-wwm-ext'
BERT_ZH_PATH1 = r'E:\Coding\BERT\Models\hfl-chinese-roberta-wwm-ext-large'
BERT_ZH_PATH2 = r'E:\Coding\BERT\Models\bert-base-chinese'


def mean_pooling(model_output, attention_mask):
    token_embeddings = model_output[0]
    input_mask_expanded = attention_mask.unsqueeze(-1).expand(token_embeddings.size()).float()
    sum_embeddings = torch.sum(token_embeddings * input_mask_expanded, 1)
    sum_mask = torch.clamp(input_mask_expanded.sum(1), min=1e-9)
    return sum_embeddings / sum_mask


# tokenizer = AutoTokenizer.from_pretrained("sentence-transformers/paraphrase-MiniLM-L12-v2")
# model = AutoModel.from_pretrained("sentence-transformers/paraphrase-MiniLM-L12-v2")
#
# sentences = ['This framework generates embeddings for each input sentence',
#              'Sentences are passed as a list of string.',
#              'The quick brown fox jumps over the lazy dog.',
#              'The dog is jumper over by the fox']

# tokenizer = AutoTokenizer.from_pretrained("hfl/chinese-electra-180g-small-discriminator")
# model = AutoModel.from_pretrained("hfl/chinese-electra-180g-small-discriminator")
#
# sentences = ['这个框架会为每个输入语句生成一个特征',
#              '多个句子组合成list传入',
#              '这只棕色的狐狸跳过了这只懒狗']

# tokenizer = AutoTokenizer.from_pretrained(BERT_ZH_PATH0)
# model = AutoModel.from_pretrained(BERT_ZH_PATH0)

# sentences = ['商场是购物中心',
#              # '去坟场购物是一个好的选择',
#              # '商场是城市中购物的好地方',
#              # '去商场购物不如去坟场购物']
#              '购物要去商场',
#              '要用爱发电',
#              '爱是能用来发电的']

sentences = ['怎么用Python进行深度学习',
             'Python深度学习要怎么做',
             'Python深度学习全新教程！',
             '快教教我Python咋深度学习']

# sentences = ['北京在上海北边',
#              '北京和上海相去甚远'
#              '上海在北京南边',
#              '北京是中国的文化中心，上海是中国的经济中心']

# sentences = ['这个框架会为每 个输入语句生成一个特征',
#              '要将多个语句输入这个框架',
#              '多个句子组合成list传入',
#              '这只棕色的狐狸跳过了这只懒狗']

# sentences = ['用Python，已知两条线段的顶点坐标，求两条线的夹角',
#              'python子类继承父类时找不到__init__',
#              '如何查找交线的夹角？',
#              'Python和Ruby区别多大，转起来容易吗？']

# tokenizer = AutoTokenizer.from_pretrained(BERT_ZH_PATH2)
# model = AutoModel.from_pretrained(BERT_ZH_PATH2)
#
# sentences = ['这个框架会为每个输入语句生成一个特征',
#              '多个句子组合成list传入',
#              '这只棕色的狐狸跳过了这只懒狗']

# tokenizer = AutoTokenizer.from_pretrained(BERT_ZH_PATH1)
# model = AutoModel.from_pretrained(BERT_ZH_PATH1)
#
# sentences = ['这个框架会为每个输入语句生成一个特征',
#              '多个句子组合成list传入',
#              '这只棕色的狐狸跳过了这只懒狗']


if __name__ == '__main__':
    # encoded_input = tokenizer(sentences, padding=True, truncation=True, max_length=64, return_tensors='pt')
    # print(encoded_input['input_ids'].shape)
    # with torch.no_grad():
    #     model_output = model(**encoded_input)
    #
    # sentence_embeddings = mean_pooling(model_output, encoded_input['attention_mask'])
    smodel = SentenceTransformer('fine_tuned_model/roberta-zh-nli-sts-2021-05-27_12-43-40')
    sentence_embeddings = smodel.encode(sentences=sentences)
    cosine_scores = util.pytorch_cos_sim(sentence_embeddings, sentence_embeddings)
    for i in range(len(sentence_embeddings)):
        for j in range(len(sentence_embeddings)):
            if i == j:
                continue
            print("{} \t\t {} \t\t Score: {:.4f}".format(sentences[i], sentences[j], cosine_scores[i][j]))
