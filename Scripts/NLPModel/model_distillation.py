from torch.utils.data import DataLoader
from sentence_transformers import models, losses, evaluation
from sentence_transformers import LoggingHandler, SentenceTransformer, util, InputExample
from sentence_transformers.datasets import ParallelSentencesDataset
import logging
from datetime import datetime
import os
import random
from sklearn.decomposition import PCA
import torch
import argparse
import const
import train_sts


def _add_sentences(dir_path, file_name, sep, indices):
    samples_set = set()
    with open(os.path.join(dir_path, file_name), 'r', encoding='utf8') as f:
        samples = f.read().splitlines()
        samples.pop(0)
        samples = list(map(lambda sentence: sentence.split(sep), samples))
    for record in samples:
        for i in indices:
            samples_set.add(record[i].strip())
    ret = list(samples_set)
    random.shuffle(ret)
    return ret


if __name__ == '__main__':
    parser = argparse.ArgumentParser(
        description='This script train a faster model with similar performance from a origin one.')
    parser.add_argument('-d', '--device', type=int, default=3, help='cuda device')
    parser.add_argument('-e', '--epoch', type=int, default=1, help='training epochs')
    parser.add_argument('-wu', '--warmup', type=int, default=1000, help='warmup steps')
    parser.add_argument('-es', '--evaluation_steps', type=int, default=5000, help='evaluation steps')
    parser.add_argument('-ibs', '--inference_batch_size', type=int, default=64, help='inference batch size')
    parser.add_argument('-tbs', '--train_batch_size', type=int, default=64, help='train_batch_size')
    parser.add_argument('-mn', '--model_name', type=str,
                        default=os.path.join(const.FINE_TUNED_MODEL_DIR, 'roberta-zh-nli-sts-2021-05-27_12-43-40'))

    args = parser.parse_args()

    # noinspection PyArgumentList
    logging.basicConfig(format='%(asctime)s - %(message)s',
                        datefmt='%Y-%m-%d %H:%M:%S',
                        level=logging.INFO,
                        handlers=[LoggingHandler()])

    device = 'cuda:{}'.format(args.device)

    teacher_model = SentenceTransformer(args.model_name, device=device)

    student_model = SentenceTransformer(args.model_name, device=device)
    auto_model = student_model._first_module().auto_model.to(device)
    layers_to_keep = [1, 4, 7, 10]
    logging.info("Remove layers from student. Only keep these layers: {}".format(layers_to_keep))
    new_layers = torch.nn.ModuleList(
        [layer_module for i, layer_module in enumerate(auto_model.encoder.layer) if i in layers_to_keep])
    auto_model.encoder.layer = new_layers
    auto_model.config.num_hidden_layer = len(layers_to_keep)

    output_path = os.path.join(const.FINE_TUNED_MODEL_DIR,
                               'distillation_{}'.format(datetime.now().strftime("%Y-%m-%d_%H-%M-%S")))
    os.mkdir(output_path)

    nli_dataset_path = os.path.join(const.CORPUS_DIR, 'modified_xnli')
    sts_dataset_path = os.path.join(const.CORPUS_DIR, 'lcqmc')
    news_dataset_path = os.path.join(const.CORPUS_DIR, 'tnews')
    logging.info("Load data from {}, {} and {}".format(nli_dataset_path, sts_dataset_path, news_dataset_path))

    train_sentences_nli = _add_sentences(nli_dataset_path, 'train.tsv', sep='\t', indices=(0, 1))
    train_sentences_sts = _add_sentences(sts_dataset_path, 'train.tsv', sep='\t', indices=(0, 1))
    train_sentences_news = _add_sentences(news_dataset_path, 'toutiao_category_train.txt', sep='_!_', indices=(3, ))
    train_data = ParallelSentencesDataset(student_model=student_model,
                                          teacher_model=teacher_model,
                                          batch_size=args.inference_batch_size, use_embedding_cache=False)
    train_data.add_dataset([[sent] for sent in train_sentences_nli], max_sentence_length=256)
    train_data.add_dataset([[sent] for sent in train_sentences_sts], max_sentence_length=256)
    train_data.add_dataset([[sent] for sent in train_sentences_news], max_sentence_length=256)
    train_dataloader = DataLoader(train_data, shuffle=True, batch_size=args.train_batch_size)
    train_loss = losses.MSELoss(model=student_model)

    dev_sentences = _add_sentences(sts_dataset_path, 'dev.tsv', sep='\t', indices=(0, 1))
    dev_sentences += _add_sentences(news_dataset_path, 'toutiao_category_dev.txt', sep='_!_', indices=(3, ))
    dev_evaluator_mse = evaluation.MSEEvaluator(dev_sentences, dev_sentences, teacher_model=teacher_model)

    dev_samples = list()
    train_sts._add_samples(sts_dataset_path, 'test.tsv', dev_samples)
    dev_evaluator_sts = evaluation.EmbeddingSimilarityEvaluator.from_input_examples(dev_samples, name='sts-dev')

    logging.info("Teacher Performance:")
    dev_evaluator_sts(teacher_model)

    if student_model.get_sentence_embedding_dimension() < teacher_model.get_sentence_embedding_dimension():
        logging.info("Student model has fewer dimensions than the teacher. Compute PCA for down projection")
        pca_sentences = train_sentences_sts[:20000] + train_sentences_nli[:20000]
        pca_embeddings = teacher_model.encode(pca_sentences, convert_to_numpy=True)
        pca = PCA(n_components=student_model.get_sentence_embedding_dimension())
        pca.fit(pca_embeddings)

        dense = models.Dense(in_features=teacher_model.get_sentence_embedding_dimension(),
                             out_features=student_model.get_sentence_embedding_dimension(), bias=False,
                             activation_function=torch.nn.Identity())
        dense.linear.weight = torch.nn.Parameter(torch.tensor(pca.components_))
        teacher_model.add_module('dense', dense)
        logging.info("Teacher Performance with {} dimensions:".format(teacher_model.get_sentence_embedding_dimension()))
        dev_evaluator_sts(teacher_model)

    student_model.fit(train_objectives=[(train_dataloader, train_loss)],
                      evaluator=evaluation.SequentialEvaluator([dev_evaluator_sts, dev_evaluator_mse]),
                      epochs=args.epoch,
                      warmup_steps=args.warmup,
                      evaluation_steps=args.evaluation_steps,
                      output_path=output_path,
                      save_best_model=True,
                      optimizer_params={'lr': 1e-4, 'eps': 1e-6, 'correct_bias': False},
                      use_amp=True)
