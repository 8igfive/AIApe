import const
import EmbeddingModel
import EmbeddingSet
import RetrievalEngine
import argparse
from flask import Flask, request, abort, make_response

parser = argparse.ArgumentParser(description='An app for sentences embedding.')
parser.add_argument('-mn', '--model_name', type=str, 
                    default='distillation-sts_2021-05-28_17-33-59', help='model name')
parser.add_argument('-msn', '--max_sentences_num', type=int, default=16, help='max sentences number')
parser.add_argument('-d', '--device', type=str, default='cuda:{}'.format(0), help='device to run model')
parser.add_argument('-ht', '--host', type=str, default='127.0.0.1', help='host to run on')
parser.add_argument('-pt', '--port', type=int, default=5000, help='port to listen on')

args = parser.parse_args()

embedding_model = EmbeddingModel.EmbeddingModel(model_name=args.model_name,
                                 max_sentences_num=args.max_sentences_num, device=args.device)

embedding_set = EmbeddingSet.EmbeddingSet(embedding_model)

retrieval_engine = RetrievalEngine.RetrievalEngine(embedding_model)

app = Flask('EmbeddingApp', root_path=const.CURRENT_PATH)

check_authentication = {'root': 'root@aiape.icu', 'lmx': 'lmx@aiape.icu', 'dxy': 'dxy@aiape.icu'}

def _check_account(body):
    return 'name' in body and \
           'password' in body and \
            body['password'] == check_authentication[body['name']]

@app.route('/api/embeddings', methods=['POST'])
def api_embeddings():
    res = {'status': '', 'message': '', 'embeddings': []}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'sentences' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'lack of sentences'
        return make_response(res, 200)
    embeddings = embedding_model.embedding(json_body['sentences']).astype(float)
    res['status'] = 'success'
    res['message'] = 'embedding succeeded'
    res['embeddings'] = [list(embedding) for embedding in embeddings]
    # print(res)
    return make_response(res, 200)

@app.route('/api/retrieval', methods=['POST'])
def api_retrieval():
    res = {'status':'', 'message': '', 'results': []}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'number' not in json_body or json_body['number'] <= 0:
        res['status'] = 'fail'
        res['message'] = 'lack of number or number illegal'
        return make_response(res, 200)
    number = json_body['number']
    if 'question' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'lack of question'
        return make_response(res, 200)
    question = json_body['question']
    if 'languages' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'lack of languages to be retrieved in'
        return make_response(res, 200)
    to_be_retrieved = embedding_set.get_languages_records(json_body['languages'])
    if to_be_retrieved is False:
        res['status'] = 'fail'
        res['message'] = 'illegal language exist(s)'
        return make_response(res, 200)
    question_embedding = embedding_model.embedding(question)
    retrieval_result = retrieval_engine.retrieve(question_embedding, to_be_retrieved[1], number)
    if retrieval_result is False:
        res['status'] = 'fail'
        res['message'] = 'number is too large'
        return make_response(res, 200)
    res['status'] = 'success'
    res['message'] = 'retrieval succeed'
    for score, idx in retrieval_result:
        res['results'].append([int(to_be_retrieved[0][idx]), float(score)])
    return make_response(res, 200)


@app.route('/api/add', methods=['POST'])
def api_add():
    res = {'status': '', 'message': '', 'embedding': []}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'language' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'language not exists'
        return make_response(res, 200)
    language = json_body['language']
    if 'qid' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'qid not exists'
        return make_response(res, 200)
    qid = json_body['qid']
    if 'question' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'question not exists'
        return make_response(res, 200)
    question = json_body['question']
    add_result = embedding_set.add(language, qid, question)
    if add_result is False:
        res['status'] = 'fail'
        res['message'] = 'language illegal'
        return make_response(res, 200)
    res['status'] = 'success'
    res['message'] = 'succeed adding question'
    res['embedding'] = list(add_result[0].astype(float))
    return make_response(res, 200)


@app.route('/api/delete', methods=['POST'])
def api_delete():
    res = {'status': '', 'message': ''}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'qid' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'qid not exists'
        return make_response(res, 200)
    qid = json_body['qid']
    delete_result = embedding_set.delete(qid)
    if delete_result is False:
        res['status'] = 'fail'
        res['message'] = 'unknown qid'
        return make_response(res, 200)
    res['status'] = 'success'
    res['message'] = 'succeed deleting question'
    return make_response(res, 200)


@app.route('/api/select', methods=['POST'])
def api_select():
    res = {'status': '', 'message': '', 'prompt': ''}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'reply' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'reply not exist'
        return make_response(res, 200)
    reply = json_body['reply']
    if 'prompts' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'prompts not exist'
        return make_response(res, 200)
    prompts = json_body['prompts']
    prompts_embeddings = embedding_set.get_prompts_embedding(prompts)
    if prompts_embeddings is False:
        res['status'] = 'fail'
        res['message'] = 'prompts don\'t contain any prompt'
        return make_response(res, 200)
    reply_embedding = embedding_model.embedding(reply)
    retrieval_result = retrieval_engine.retrieve(reply_embedding, prompts_embeddings, 1)
    if retrieval_result is False:
        assert False
    res['status'] = 'success'
    res['message'] = 'succeed selecting question'
    res['prompt'] = prompts[retrieval_result[0][1]]
    return make_response(res, 200)


@app.route('/api/dump', methods=['POST'])
def api_dump():
    res = {'status': '', 'message': '', 'count': {}}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    res['status'] = 'success'
    res['message'] = 'dump succeed'
    res['count'] = embedding_set.dump()
    return make_response(res, 200)

@app.route('/api/checkqids', methods=['POST'])
def api_checkqids():
    res = {'status': '', 'message': '', 'qids': []}
    json_body = request.json
    if not _check_account(json_body):
        res['status'] = 'fail'
        res['message'] = 'authentication failed'
        return make_response(res, 401)
    if 'qids' not in json_body:
        res['status'] = 'fail'
        res['message'] = 'qids not exist'
        return make_response(res, 200)
    qids = json_body['qids']
    checkqids_result = embedding_set.checkqids(qids)
    res['status'] = 'success'
    res['message'] = 'succeeding checking qids.'
    res['qids'] = checkqids_result
    return make_response(res, 200)

if __name__ == '__main__':
    app.run(debug=True, host=args.host, port=args.port)