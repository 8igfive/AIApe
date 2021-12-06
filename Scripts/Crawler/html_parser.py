import markdown_rander
import const
import os
import json
from bs4 import BeautifulSoup

class HTMLParser:
    def __init__(self, rander=markdown_rander.MarkdownRander()):
        self.tag_map = {'c': 'C', 'java': "Java", 'python': 'Python', 'sql': 'SQL'}
        self.rander = rander
        self.count = {'C':0, 'Java': 0, 'Python': 0, 'SQL': 0, 'Other': 0}
        for dir_name in self.count:
            self.count[dir_name] = len(os.listdir(os.path.join(const.CRAWLER_RESULT_PATH, dir_name)))
    
    def parse_questions(self, html, base_url):
        soup = BeautifulSoup(html, features='lxml')
        questions = soup.find_all('h2', class_='title')
        res = list()
        for question in questions:
            res.append(base_url + question.a.attrs['href'])
        return res

    def parse_detail(self, html, dump=True):
        soup = BeautifulSoup(html, features='lxml')
        self.question_title = soup.find_all('a', class_='text-body')[0].string
        qas = soup.find_all('article', class_='article fmt article-content')
        if len(qas) < 2:
            return
        self.question_content = self.rander.rander_article(qas[0])
        self.answer_content = self.rander.rander_article(qas[1])
        raw_tags = soup.find_all('a', class_='m-1 badge-tag')
        self.tags = list()
        for t in raw_tags:
            self.tags.append(str(t.string))
        for i in range(len(self.tags)):
            if self.tags[i] in self.tag_map:
                self.tags[i] = self.tag_map[self.tags[i]]
        if dump:
            self._dump()
    
    def _fix_json(self, language_dir, json_name):
        with open(os.path.join(const.CRAWLER_RESULT_PATH, language_dir, json_name), 'r', encoding='utf8') as f:
            json_content = json.load(f)
        json_content['question_content'] = self.rander.fix_markdown(json_content['question_content'])
        json_content['answer_content'] = self.rander.fix_markdown(json_content['answer_content'])
        with open(os.path.join(const.CRAWLER_RESULT_PATH, language_dir, json_name), 'w', encoding='utf8') as f:
            json.dump(json_content, f, indent=4, ensure_ascii=False)

    def _dump(self):
        dir_name = 'Other'
        for name in {'C', 'Python', 'Java', 'SQL'}:
            if name in self.tags:
                dir_name = name
                break
        with open(os.path.join(const.CRAWLER_RESULT_PATH, dir_name, '{}.json'.format(self.count[dir_name])), 'w', encoding='utf8') as f:
            json.dump({
                'question_title': self.question_title,
                'question_content': self.question_content,
                'answer_content': self.answer_content,
                'tags': self.tags
            }, f, indent=4, ensure_ascii=False)
        self.count[dir_name] += 1

    def fix_dataset(self, languages=('C', 'Java', 'Python', 'SQL', 'Other')):
        for language in languages:
            files = os.listdir(os.path.join(const.CRAWLER_RESULT_PATH, language))
            for file in files:
                self._fix_json(language, file)