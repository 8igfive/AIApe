import json
import os
import matplotlib.pyplot as plt
import seaborn as sns
import pdb
from utils.Utils import LogLevel, ensure_and_clean_up_dir, log, pcat
from utils.Config import Path, relative_path
from utils.Poster import login, get
from .TagManager import get_remote_tag_name2tid_dict
from tqdm import tqdm

plt.rcParams['font.sans-serif'] = ['SimHei']
plt.rcParams['axes.unicode_minus'] = False

SUMMARY_FILENAME = 'summary.json'
DETAIL_FILENAME = 'detail.json'

tags = None
qas = None


def _load_remote_tags():
    log('load remote tags', level=LogLevel.INF)
    global tags
    tags = dict()
    tids = get_remote_tag_name2tid_dict()
    for tag, tid in tqdm(tids.items()):
        tag_info = get(f'/api/questions/tag?tid={tid}')
        try:
            category = tag_info['tag']['category']
        except:
            log(f'error in getting information of tag({tag})', level=LogLevel.ERR)
            continue
        if category not in tags:
            tags[category] = list()
        tags[category].append(tag)

def _load_local_qas():
    log('load local qas', level=LogLevel.INF)
    global qas
    qas = dict()
    pure_data = pcat(Path.Scripte_DataManager_Datas, 'pure')
    categorys = os.listdir(pure_data)
    autotags = {
        category: []
        for category in categorys
    }
    autotag_path = pcat(Path.Scripte_DataManager_Datas, 'auto_tag')
    selects = autotags.copy()
    select_path = pcat(Path.Scripte_DataManager_Datas, 'select')
    for category in categorys:
        if category not in qas:
            qas[category] = list()
        category_path = pcat(pure_data, category)
        questions = os.listdir(category_path)
        autotags[category] = os.listdir(pcat(autotag_path, category))
        selects[category] = os.listdir(pcat(select_path, category))
        log(f'load questions from category({category})', level=LogLevel.INF)
        for question in tqdm(questions):
            with open(pcat(category_path, question), 'r', encoding='utf8') as fin:
                question_info = json.load(fin)
            if question in autotags[category]:
                with open(pcat(pcat(autotag_path, category), question), 'r', encoding='utf8') as fin:
                    add_tags = json.load(fin)
                question_info['tags'] = list(set(question_info['tags'] + add_tags))
            if question in selects[category]:
                with open(pcat(pcat(select_path, category), question), 'r', encoding='utf8') as fin:
                    add_tags = json.load(fin)['add_tags']
                question_info['tags'] = list(set(question_info['tags'] + add_tags))
            qas[category].append({
                'title_length': len(question_info['title']),
                'remarks_length': len(question_info['remarks']),
                'tags': question_info['tags']
            })

def _init(qas_src='local'):
    global tags, qas
    if tags is None:
        _load_remote_tags()
    if qas is None:
        if qas_src != 'local':
            log('can not get remote qas : not implemented', level=LogLevel.ERR)
        _load_local_qas()

def _compute_summary():
    log('compute summary', level=LogLevel.INF)
    global tags, qas
    tag_count = dict()
    for _, questions in qas.items():
        for question in tqdm(questions):
            for tag in question['tags']:
                if tag not in tag_count:
                    tag_count[tag] = 0
                tag_count[tag] += 1
    res = dict()
    # pdb.set_trace() # FIXME
    for category in tags:
        res[category] = dict()

        for tag in tags[category]:
            if tag not in tag_count:
                log(f'no information for tag({tag})', level=LogLevel.WAR)
            else:
                res[category][tag] = tag_count[tag]
    return res

def _compute_detail():
    log('compute detail', level=LogLevel.INF)
    global tags, qas
    res = dict()
    for category in tags:
        for tag in tags[category]:
            res[tag] = {
                'title_length': [],
                'remarks_length': [],
            }
    for tag in res:
        res[tag]['tag_distribution'] = {
            inner_tag : 0
            for inner_tag in filter(
                lambda it: it != tag,
                res
            )
        }
    for _, questions in qas.items():
        for question in tqdm(questions):
            for tag in question['tags']:
                if tag not in res:
                    # log(f'unknown tag({tag})', level=LogLevel.WAR)
                    continue
                res[tag]['title_length'].append(question['title_length'])
                res[tag]['remarks_length'].append(question['remarks_length'])
                inner_tags = list(
                    filter(
                        lambda inner_tag: inner_tag != tag and inner_tag in res,
                        question['tags']
                    )
                )
                for inner_tag in inner_tags:
                    res[tag]['tag_distribution'][inner_tag] += 1
    return res

def _barplot(name, data):
    x = list(map(str, data))
    y = list(map(lambda i: data[i], data))
    sns.barplot(x, y)
    plt.xticks(rotation=50)
    plt.savefig(f'{name}_bar.png')
    plt.show()

def _kdeplot(name, data):
    # pdb.set_trace()
    sns.kdeplot(data, shade=True)
    plt.xticks(rotation=50)
    plt.savefig(f'{name}_kde.png')
    plt.show()

def _plot_summary(summary):
    log('plot summary', level=LogLevel.INF)
    for category in summary:
        _barplot(pcat(pcat(Path.Scripte_DataManager_Datas, 'statistics'), f'summary_{category}'), summary[category])

def _plot_detail(detail):
    log('plot detail', level=LogLevel.INF)
    for tag in detail:
        _kdeplot(pcat(pcat(Path.Scripte_DataManager_Datas, 'statistics'), f'detail_{tag}_title_length'), detail[tag]['title_length'])
        _kdeplot(pcat(pcat(Path.Scripte_DataManager_Datas, 'statistics'), f'detail_{tag}_remarks_length'), detail[tag]['remarks_length'])
        _barplot(pcat(pcat(Path.Scripte_DataManager_Datas, 'statistics'), f'detail_{tag}_tags_dist'), detail[tag]['tag_distribution'])

def _generate_markdown(categories, tags):
    statistics_path = pcat(Path.Scripte_DataManager_Datas, 'statistics')
    h3_format = '### {}\n\n'
    h4_format = '#### {}\n\n'
    summary_graph_format = '![{}]({})\n\n'
    detail_graph_format = '<img src="{}" alt="{}" style="zoom:33%;" />'
    summary_file_format = 'summary_{}_bar.png'
    detail_title_length_file_format = 'detail_{}_title_length_kde.png'
    detail_remarks_length_file_format = 'detail_{}_remarks_length_kde.png'
    detail_tags_dist_file_format = 'detail_{}_tags_dist_bar.png'
    with open(pcat(statistics_path, 'statistics.md'), 'w', encoding='utf8') as fout:
        fout.write(h3_format.format('Summary'))
        for category in categories:
            fout.write(h4_format.format(category))
            summary_name = summary_file_format.format(category)
            summary_file_path = os.path.abspath(pcat(statistics_path, summary_name))
            fout.write(summary_graph_format.format(
                summary_file_path, summary_name
            ))
        fout.write(h3_format.format('Detail'))
        for tag in tags:
            fout.write(h4_format.format(tag))
            for detail_file_format in [
                detail_title_length_file_format,
                detail_remarks_length_file_format,
                detail_tags_dist_file_format
            ]:
                detail_name = detail_file_format.format(tag)
                detail_file_path = os.path.abspath(pcat(statistics_path, detail_name))
                fout.write(detail_graph_format.format(
                    detail_file_path, detail_name
                ))
            fout.write('\n\n')
def compute_statistics():
    load_or_dump = 'dump'
    generate_graph = False
    generate_md = False

    ld = 'None'
    while ld.lower() not in ['l', 'load', 'd', 'dump', '']:
        ld = input('whether to load existing statistics or generate and dump one [load/Dump]')
    if ld.lower() == 'load' or ld.lower() == 'l':
        load_or_dump = 'load'

    gg = 'None'
    while gg.lower() not in ['y', 'yes', 'n', 'no', '']:
        gg = input('whether to generate and save statistics graph [y/N]')
    if gg.lower() == 'y' or gg.lower() == 'Y':
        generate_graph = True
    
    gm = 'None'
    while gm.lower() not in ['y', 'yes', 'n', 'no', '']:
        gm = input('whether to generate markdown [y/N]')
    if gm.lower() == 'y' or gm.lower() == 'yes':
        generate_md = True

    statistics_path = pcat(Path.Scripte_DataManager_Datas, 'statistics')
    if load_or_dump == 'load':
        statistics_path = pcat(Path.Scripte_DataManager_Datas, 'statistics')
        if SUMMARY_FILENAME not in os.listdir(statistics_path) or \
           DETAIL_FILENAME not in os.listdir(statistics_path):
            log('try to load statistics but missing necessary file, change to generate statistics', level=LogLevel.WAR)
            load_or_dump = 'dump'
        if load_or_dump == 'load':
            with open(pcat(statistics_path, SUMMARY_FILENAME), 'r', encoding='utf8') as fin:
                summary = json.load(fin)
            with open(pcat(statistics_path, DETAIL_FILENAME), 'r', encoding='utf8') as fin:
                detail = json.load(fin)

    if load_or_dump == 'dump':
        _init()
        summary = _compute_summary()
        detail = _compute_detail()
        with open(pcat(statistics_path, SUMMARY_FILENAME), 'w', encoding='utf8') as fout:
            json.dump(summary, fout, ensure_ascii=False, indent=4)
        with open(pcat(statistics_path, DETAIL_FILENAME), 'w', encoding='utf8') as fout:
            json.dump(detail, fout, ensure_ascii=False, indent=4)
    
    if generate_graph:
        _plot_summary(summary)
        _plot_detail(detail)

    if generate_md:
        _generate_markdown(summary.keys(), detail.keys())