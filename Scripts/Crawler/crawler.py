import const
from html_parser import HTMLParser
import os
import time
import requests
import pickle
import logging
import datetime
import numpy as np
from selenium import webdriver
class StatusNotOkException(Exception):
    def __init__(self, url, code):
        self.url = url
        self.code = code
    def __str__(self):
        '请求失败\n\t状态码为: {}\n\turl为: {}'.format(self.code, self.url)

class Crawler:
    def __init__(self, parser=HTMLParser()):
        self.params = {
            'Accept': '*/*',
            'Accept-Encoding': 'gzip, deflate, br',
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36 Edg/90.0.818.51',
            'Connection': 'keep-alive'
        }
        self.cookies = {}
        self.parser = parser
        self.base_url = 'https://segmentfault.com'
        self.questions_url = 'https://segmentfault.com/t/{}/questions?type=votes&page={}'
        self.detail_urls = list()
        self.detail_urls_dump_name = 'detail_u_d_n.pkl'
        if self.detail_urls_dump_name in os.listdir(const.TEMP_PATH):
            with open(os.path.join(const.TEMP_PATH, self.detail_urls_dump_name), 'rb') as f:
                self.detail_urls = pickle.load(f)
        self.done_urls = set()
        self.done_urls_dump_name = 'dump_u_d_n.pkl'
        if self.done_urls_dump_name in os.listdir(const.TEMP_PATH):
            with open(os.path.join(const.TEMP_PATH, self.done_urls_dump_name), 'rb') as f:
                self.done_urls = pickle.load(f)
        self.languages = ['java', 'python', 'c', 'sql']
        self._init_browser()
    
    def _init_browser(self):
        logging.info('start to init browser')
        chrome_options = webdriver.ChromeOptions()
        chrome_options.add_argument('--headless')
        chrome_options.add_argument('--disable-gpu')

        self.browser = webdriver.Chrome(chrome_options=chrome_options)
        logging.info('init browser successfully')

    def start(self, languages=None, max_page=100):
        if languages == None:
            languages = self.languages
        languages = languages and self.languages
        if len(self.detail_urls) == 0:
            self._crawl_questions(languages, max_page)               
        self._crawl_details()

    def _crawl_questions(self, languages, max_page):
        for language in languages:
            logging.info('start to crawl questions of {}'.format(language))
            for page in range(1, max_page + 1):
                logging.info('start to crawl page {}'.format(page))
                url = self.questions_url.format(language, page)
                try:
                    self._get(url)
                except StatusNotOkException as e:
                    print(e)
                    break
                self.detail_urls += self.parser.parse_questions(self.page_source, self.base_url)
                time.sleep(np.random.choice(a=[1, 2, 3], size=1, p=[0.6, 0.3, 0.1]).item())
        with open(os.path.join(const.TEMP_PATH, self.detail_urls_dump_name), 'wb') as f:
            pickle.dump(self.detail_urls, f)
    
    def _crawl_details(self):
        logging.info('start to crawl details with a number of {}'.format(len(self.detail_urls)))
        for url in self.detail_urls:
            if url not in self.done_urls:
                logging.info('start to crawl {}\'s detail'.format(url))
                try:
                    self._get(url)
                except StatusNotOkException as e:
                    print(e)
                    break
                try:
                    self.parser.parse_detail(self.page_source)
                    self.done_urls.add(url)
                except Exception as e:
                    logging.error('error when parse {}, error message is {}'.format(url, e))
                    break
                time.sleep(np.random.choice(a=[2, 3, 4], size=1, p=[0.6, 0.3, 0.1]).item())
        with open(os.path.join(const.TEMP_PATH, self.done_urls_dump_name), 'wb') as f:
            pickle.dump(self.done_urls, f)

    def _get(self, url, use_browser=True):
        if use_browser:
            self.browser.get(url)
            self.page_source = self.browser.page_source
        else:
            self.response = requests.get(url, params=self.params, cookies=self.cookies)
            if self.response.status_code != 200:
                logging.error('get {} failed with code {}'.format(url, self.response.status_code))
                raise StatusNotOkException(url, self.response.status_code)
            if not self.response.cookies is None:
                self.cookies = self.response.cookies
            self.response.encoding='utf-8'
            self.page_source = self.response.text
                    

if __name__ == '__main__':
    LOG_FORMAT = "%(asctime)s - %(levelname)s - %(message)s"
    DATE_FORMAT = "%m/%d/%Y %H:%M:%S %p"
    logging.basicConfig(filename=os.path.join(const.LOG_PATH, '{}.log'.format(datetime.datetime.now().strftime('%Y-%m-%d'))),
                        level=logging.INFO, format=LOG_FORMAT, datefmt=DATE_FORMAT)
    # crawler = Crawler()
    # crawler.start()
    HTMLParser().fix_dataset()
