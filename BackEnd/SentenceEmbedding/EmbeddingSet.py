import enum
import const
import os
import pickle
import datetime
import numpy as np


class EmbeddingSet:
    def __init__(self, model, languages=('C', 'Java', 'Python', 'SQL', 'Natrual', 'Other'), dump_seconds = 10):
        self.model = model
        self.cache_name = '{}_EmbeddingSet.pkl'.format(self.model.model_name)
        self.add_filename = 'QuestionAddFile.csv'
        self.delete_filename = 'QuestionDeleteFile.csv'

        if self.cache_name in os.listdir(const.CACHE_PATH):
            with open(os.path.join(const.CACHE_PATH, self.cache_name), 'rb') as f:
                self.data = pickle.load(f)
            for language in languages:
                if language not in self.data:
                    self.data[language] = [[], []]
        else:
            self.data = dict([[language, [[], []]] for language in languages])
            self.data['prompts'] = dict()
        
        self.dump_seconds = dump_seconds
        self.last_dump_time = datetime.datetime.now()
    
    def dump(self):
        with open(os.path.join(const.CACHE_PATH, self.cache_name), 'wb') as f:
            pickle.dump(self.data, f)
        return dict([(language, len(self.data[language][0])) \
                     for language in self.data if language != 'prompts'])
    
    def add(self, language, qid, question):
        if language not in self.data:
            return False
        with open(os.path.join(const.CACHE_PATH, self.add_filename), 'a', encoding='utf8') as f:
            f.write('{},{},{}\n'.format(language, qid, question))
        self.data[language][0].append(qid)
        self.data[language][1].append(self.model.embedding(question))
        self._update()
        return self.data[language][1][-1]

    
    def delete(self, qid):
        flag = False
        for language, records in self.data.items():
            if language == 'prompts':
                continue
            del_positions = list()
            for i, temp_qid in enumerate(records[0]):
                if qid == temp_qid:
                    flag = True
                    del_positions.append(i)
            for i, position in enumerate(del_positions):
                records[0].pop(position - i)
                records[1].pop(position - i)
        if flag:
            with open(os.path.join(const.CACHE_PATH, self.delete_filename), 'a', encoding='utf8') as f:
                            f.write('{}\n'.format(qid))
        self._update()
        return flag

    def checkqids(self, qids):
        to_be_checked = set(qids)
        for language, records in self.data.items():
            if language != 'prompts':
                for qid in records[0]:
                    if len(to_be_checked) == 0:
                        break
                    if qid in to_be_checked:
                        to_be_checked.remove(qid)
        return list(to_be_checked)


    def get_prompts_embedding(self, prompts):
        ret = False
        if len(prompts) == 0:
            return False
        for prompt in prompts:
            if prompt not in self.data['prompts']:
                self.data['prompts'][prompt] = self.model.embedding(prompt)
            if ret is False:
                ret = self.data['prompts'][prompt]
            else:
                ret = np.concatenate([ret, self.data['prompts'][prompt]], axis=0)
        self._update()
        return ret
    
    def get_languages_records(self, languages):
        ret = False
        for language in languages:
            if language == 'prompts' or language not in self.data:
                return False
            if ret == False:
                ret = [[], []]
            record = self.data[language]
            ret[0] += record[0]
            ret[1] += record[1]
        if not ret == False:
            ret[1] = np.concatenate(ret[1], axis=0)
        self._update()
        return ret

    def _update(self):
        present = datetime.datetime.now()
        if (present - self.last_dump_time).seconds > self.dump_seconds:
            self.dump()
            self.last_dump_time = present
