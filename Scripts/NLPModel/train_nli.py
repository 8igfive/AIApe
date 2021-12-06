import math
from sentence_transformers import models, losses, datasets
from sentence_transformers import LoggingHandler, SentenceTransformer, util, InputExample
from sentence_transformers.evaluation import EmbeddingSimilarityEvaluator
import logging
from datetime import datetime
import os
import random
import argparse
import const


def _add_samples(file, data):
    for record in file:
        # print(record)
        premise, hypo, label = map(str.strip, record)
        if premise not in data:
            data[premise] = {'contradictory': set(), 'entailment': set(), 'neutral': set()}
        data[premise][label].add(hypo)


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description='this script will train a transformer model in NLI dataset')
    parser.add_argument('-d', '--device', type=int, default=3, help='cuda device index')
    parser.add_argument('-mn', '--model_name', type=str, default='hfl/chinese-roberta-wwm-ext',
                        help='transformer model')
    parser.add_argument('-bs', '--batch_size', type=int, default=48, help='train batch size')
    parser.add_argument('-msl', '--max_seq_length', type=int, default=56, help='max sequence length')
    parser.add_argument('-e', '--epoch', type=int, default=3, help='num epochs')
    args = parser.parse_args()

    FINE_TUNED_MODEL_DIR = const.FINE_TUNED_MODEL_DIR
    CORPUS_DIR = const.CORPUS_DIR

    # noinspection PyArgumentList
    logging.basicConfig(format='%(asctime)s - %(message)s', datefmt='%Y-%m-%d %H:%M:%S',
                        level=logging.INFO, handlers=[LoggingHandler()])

    random.seed(0)

    model_name = 'hfl/chinese-roberta-wwm-ext'
    train_batch_size = args.batch_size
    max_seq_length = args.max_seq_length
    num_epochs = args.epoch
    device = 'cuda:{}'.format(args.device)

    model_save_path = os.path.join(FINE_TUNED_MODEL_DIR, 'roberta-zh-nli-{}'.
                                   format(datetime.now().strftime("%Y-%m-%d_%H-%M-%S")))
    os.mkdir(model_save_path)

    word_embedding_model = models.Transformer(model_name, max_seq_length=max_seq_length)
    pooling_model = models.Pooling(word_embedding_model.get_word_embedding_dimension(), pooling_mode='mean')
    model = SentenceTransformer(modules=[word_embedding_model, pooling_model], device=device)

    nli_dataset_path = os.path.join(CORPUS_DIR, 'modified_xnli')
    sts_dataset_path = os.path.join(CORPUS_DIR, 'lcqmc')
    dataset_files = ['dev.tsv', 'test.tsv', 'train.tsv']

    logging.info("Read XNLI train dataset")

    train_data = dict()
    with open(os.path.join(nli_dataset_path, 'train.tsv'), 'r', encoding='utf8') as f:
        train_tsv = f.read().splitlines()
        train_tsv.pop(0)
        train_tsv = list(map(lambda sentence: sentence.split('\t'), train_tsv))
    _add_samples(train_tsv, train_data)

    train_samples = list()
    for sent1, others in train_data.items():
        if len(others['contradictory']) > 0 and len(others['entailment']) > 0:
            train_samples.append(InputExample(texts=[sent1, random.choice(list(others['entailment'])),
                                                     random.choice(list(others['contradictory']))]))
            train_samples.append(InputExample(texts=[random.choice(list(others['entailment'])), sent1,
                                                     random.choice(list(others['contradictory']))]))

    logging.info("Train samples: {}".format(len(train_samples)))

    train_dataloader = datasets.NoDuplicatesDataLoader(train_samples, batch_size=train_batch_size)

    train_loss = losses.MultipleNegativesRankingLoss(model)

    logging.info("Read lcqmc dev dataset")
    dev_samples = list()
    with open(os.path.join(sts_dataset_path, 'dev.tsv'), 'r', encoding='utf8') as f:
        dev_tsv = f.read().splitlines()
        dev_tsv.pop(0)
        dev_tsv = list(map(lambda sentence: sentence.split('\t'), dev_tsv))
    for record in dev_tsv:
        dev_samples.append(InputExample(texts=[record[0], record[1]], label=float(record[2])))

    dev_evaluator = EmbeddingSimilarityEvaluator.from_input_examples(dev_samples, batch_size=train_batch_size,
                                                                     name='sts-dev')
    dev_evaluator(model, output_path=model_save_path)

    warmup_steps = math.ceil(len(train_dataloader) * num_epochs * 0.1)  # 10% of train data for warm-up
    logging.info("Warmup-steps: {}".format(warmup_steps))
    model.fit(train_objectives=[(train_dataloader, train_loss)],
              evaluator=dev_evaluator,
              epochs=num_epochs,
              evaluation_steps=int(len(train_dataloader) * 0.1),
              warmup_steps=warmup_steps,
              output_path=model_save_path,
              use_amp=True  # Set to True, if your GPU supports FP16 operations
              )

    logging.info("Read lcqmc test dataset")
    test_samples = list()
    with open(os.path.join(sts_dataset_path, 'test.tsv'), 'r', encoding='utf8') as f:
        test_tsv = f.read().splitlines()
        test_tsv.pop(0)
        test_tsv = list(map(lambda sentence: sentence.split('\t'), test_tsv))
    for record in test_tsv:
        test_samples.append(InputExample(texts=[record[0], record[1]], label=float(record[2])))

    model = SentenceTransformer(model_save_path)
    test_evaluator = EmbeddingSimilarityEvaluator.from_input_examples(test_samples, batch_size=train_batch_size,
                                                                      name='sts-test')
    test_evaluator(model, output_path=model_save_path)
