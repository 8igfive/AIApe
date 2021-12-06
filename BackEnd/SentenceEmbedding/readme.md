# 序

## 目录结构

```
/
|---Cache/
|---Corpus/
|---Demo/
|---Models/
|---.gitignore
|---const.py
|---EmbeddingApp.py
|---EmbeddingModel.py
|---EmbeddingSet.py
|---readme.md
|---RetrievalEngine.py
|---search_simulation.py
```

## 依赖包

* pytorch
* transformers
* sentence-transformers
* flask

# 句子特征获取微服务

## 获取句子特征 /api/embeddings

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "sentences" : ["sentence1", "sentence2"]
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `sentences`：要进行特征提取的多个句子。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "embeddings successfully get",
      "embeddings" : [[0.321317, 1.302184], [0.489172, 4.371628]]
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `embeddings`：对每个句子生成的特征。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|缺少语句|
|`fail`|`401 Unauthorized`|权限验证失败|

## 检索问题 /api/retrieval

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "languages" : ["C", "Java", "Python", "SQL", "Natrual"],
      "presorting" : false,
      "question" : "a question",
      "number" : 30
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `languages`：要检索的语言范围。
  * `presorting`：是否需要进行预分类，目前不支持进行预分类。
  * `question`：要进行检索的问题
  * `number`：需要返回的问题数量
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "retrieval succeed",
      "results" : [[1039, 0.94], [513, 0.93], [741, 0.90]]
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `results`：检索的结果，对每一项结果，第一个数据表示qid，第二个数据表示检索得分。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|`languages`不能为空或有非法成分或`number`不能为0且不能超过可检索范围或缺少`question`|
|`fail`|`401 Unauthorized`|权限验证失败|

## 添加问题 /api/add

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "language" : "C",
      "qid" : 666,
      "question" : "a question",
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `language`：问题的语言类别。
  * `qid`：问题的qid。
  * `question`：要添加的问题。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "succeed adding question",
      "embedding" : [0.12934, 0.412983, 1.381293]
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `embedding`：对添加的问题生成的embedding。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|`language`非法|
|`fail`|`401 Unauthorized`|权限验证失败|

## 删除问题 /api/delete

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "qid" : 666,
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `qid` : 要删除的问题的qid。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "succeed deleting question",
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|数据中无法找到符合该qid的项。|
|`fail`|`401 Unauthorized`|权限验证失败|

## 分支选择 /api/select

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "reply" : "balabalabala",
      "prompts" : ["prompt0", "prompt1", "prompt2"]
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `reply` : 用户的回应。
  * `prompts` : 每个分支的标准回答。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "succeed selecting question",
      "prompt" : "prompt1"
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `prompt`：根据语义选择的分支。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|`prompts`为空|
|`fail`|`401 Unauthorized`|权限验证失败|

## 缓存问题 /api/dump

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "succeed selecting question",
      "count" : {
        "C" : 10,
        "Java" : 10,
        "Python" : 10,
        "SQL" : 10,
        "Natrual" : 10,
        "Other" : 10
      }
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `count`：各个语言目前拥有的问题数。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`401 Unauthorized`|权限验证失败|

## 检查qid是否存在 /api/checkqids

* 类型：POST
* 请求体：
  ```JSON
  {
      "name" : "root",
      "password" : "root@aiape.icu",
      "qids" : [123, 312, 43]
  }
  ```
* 请求体各部分含义：
  * `name`：管理员账户。
  * `password`：账户密码。
  * `qids`：包含要检查的qid的列表。
* 响应体：
  ```JSON
  {
      "status" : "success",
      "message" : "succeed selecting question",
      "qids" : [123, 43]
  }
  ```
* 响应体各参数含义：
  * `status`：表示处理状态，`success`表示成功，其余表示失败。
  * `message`：对`status`的补充。
  * `qids`：由还未添加的qid组成的列表。
* `status`取值及其含义：

|取值|状态码|含义| 
|:-|:-|:-|
|`success`|`200 OK`|成功|
|`fail`|`200 OK`|请求体中缺少qids|
|`fail`|`401 Unauthorized`|权限验证失败|