from torch.utils.data import DataLoader
import math
from sentence_transformers import SentenceTransformer, LoggingHandler, losses, util, InputExample
from sentence_transformers.evaluation import EmbeddingSimilarityEvaluator
import logging
from datetime import datetime
import os
import argparse
import const


def _add_samples(dir_path, file_name, store_list):
    with open(os.path.join(dir_path, file_name), 'r', encoding='utf8') as f:
        tsv_content = f.read().splitlines()
        tsv_content.pop(0)
        tsv_content = list(map(lambda sentence: sentence.split('\t'), tsv_content))
    for record in tsv_content:
        store_list.append(InputExample(texts=[record[0], record[1]], label=float(record[2])))


if __name__ == '__main__':
    print('Assume the model has been fine-tuned in the NLI dataset')

    CURRENT_DIR = const.CURRENT_DIR
    FINE_TUNED_MODEL_DIR = const.FINE_TUNED_MODEL_DIR
    CORPUS_DIR = const.CORPUS_DIR

    parser = argparse.ArgumentParser(description='this script will train model in lcqmc dataset')
    parser.add_argument('-d', '--device', type=int, default=3, help='cuda device index')
    parser.add_argument('-mn', '--model_name', type=str,
                        default=os.path.join(FINE_TUNED_MODEL_DIR, 'roberta-zh-nli-2021-05-25_22-59-49'),
                        help='transformer model')
    parser.add_argument('-msp', '--model_save_path', type=str, default='roberta-zh-nli-sts', help='model save path')
    parser.add_argument('-bs', '--batch_size', type=int, default=16, help='train batch size')
    parser.add_argument('-e', '--epoch', type=int, default=4, help='num epochs')
    args = parser.parse_args()

    # noinspection PyArgumentList
    logging.basicConfig(format='%(asctime)s - %(message)s',
                        datefmt='%Y-%m-%d %H:%M:%S',
                        level=logging.INFO,
                        handlers=[LoggingHandler()])

    sts_dataset_path = os.path.join(CORPUS_DIR, 'lcqmc')

    device = 'cuda:{}'.format(args.device)
    model_name = args.model_name
    train_batch_size = args.batch_size
    num_epochs = args.epoch

    model_save_path = os.path.join(FINE_TUNED_MODEL_DIR, '{}_{}'.format(args.model_save_path, datetime.now().strftime(
        "%Y-%m-%d_%H-%M-%S")))
    os.mkdir(model_save_path)

    model = SentenceTransformer(model_name, device=device)

    train_samples = list()
    dev_samples = list()
    test_samples = list()
    logging.info("Read lcqmc train dataset")
    _add_samples(sts_dataset_path, 'train.tsv', train_samples)
    logging.info("Read lcqmc dev dataset")
    _add_samples(sts_dataset_path, 'dev.tsv', dev_samples)
    logging.info("Read lcqmc test dataset")
    _add_samples(sts_dataset_path, 'test.tsv', test_samples)

    train_dataloader = DataLoader(train_samples, shuffle=True, batch_size=train_batch_size)
    train_loss = losses.CosineSimilarityLoss(model=model)

    evaluator = EmbeddingSimilarityEvaluator.from_input_examples(dev_samples, name='sts-dev')

    warmup_steps = math.ceil(len(train_dataloader) * num_epochs * 0.1)  # 10% of train data for warm-up
    logging.info("Warmup-steps: {}".format(warmup_steps))

    model.fit(train_objectives=[(train_dataloader, train_loss)],
              evaluator=evaluator,
              epochs=num_epochs,
              evaluation_steps=1000,
              warmup_steps=warmup_steps,
              output_path=model_save_path)

    model = SentenceTransformer(model_save_path)
    test_evaluator = EmbeddingSimilarityEvaluator.from_input_examples(test_samples, name='sts-test')
    test_evaluator(model, output_path=model_save_path)
