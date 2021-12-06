import const
import re
import os
import requests
import json
from markdown_rander import MarkdownRander

# params = {
#     'Accept': '*/*',
#     'Accept-Encoding': 'gzip, deflate, br',
#     'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36',
#     'Connection': 'keep-alive'
# }

# url = 'https://segmentfault.com/q/1010000000643367'

# r = requests.get(url, params=params)
# r.encoding='utf-8'
# r.cookies

rander = MarkdownRander()

with open(os.path.join(const.TEMP_PATH, 'question.md'), 'r', encoding='utf8') as f:
    md = f.read()

new_md = rander.fix_markdown(md)

with open(os.path.join(const.TEMP_PATH, 'new_question.md'), 'w', encoding='utf8') as f:
    f.write(new_md)

# with open(os.path.join(const.CRAWLER_RESULT_PATH, 'Java', '2.json'), 'r', encoding='utf8') as f:
#     json_content = json.load(f)

# with open(os.path.join(const.TEMP_PATH, 'question.md'), 'w', encoding='utf8') as f:
#     f.write(json_content['question_content'])
