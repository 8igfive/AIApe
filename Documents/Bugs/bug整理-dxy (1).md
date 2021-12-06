


---

* 位置: Buaa.AIBot.Repository.Implement.QuestionRepository.UpdateQuestionAsync
* 级别: 一般情况下, 导致陷入死循环.
* 描述: 更新问题信息时, 会导致系统陷入死循环.
* 原因: 这是由于这一部分是拷贝InsertQuestionAsync的实现然后进行修改的, 而后者创建了QuestionData对象后, 需要将其插入数据库, 但前者是不需要, 由于插入的重试机制, 将无限重试, 陷入死循环.
* 解决情况: 顺利解决. 同时引入了重试的超时机制, 确保类似的问题不会陷入死循环.

---

* 位置: Buaa.AIBot.Repository.Implement.QuestionRepository.UpdateQuestionAsync
* 级别: 特殊情况下, 后端报错.
* 描述: 当不更新问题的标签时, 将抛出空引用异常.
* 原因: QuestionRepository通过Buaa.AIBot.Repository.Implement.QuestionRepository.TagMatcher进行标签匹配, TagMatcher在创建时需要通过传入的Tags列表初始化一个HashSet, 而当不更新Tags时, Tags列表为null, 此时将会报错.
* 解决情况: 顺利解决.


---

* 位置: Buaa.AIBot.Repository.Implement.QuestionRepository.UpdateQuestionAsync
* 级别: 特殊情况下, 发生一致性错误, 导致语义错误.
* 描述: 当更新问题信息中的最佳回答信息时, 若该回答并不属于该问题时仍能够成功, 此时一个问题的最佳回答并不属于该问题, 语义错误.
* 原因: 更新时仅检查了回答是否存在, 没有检查该回答所属问题是不是本回答.
* 解决情况: 顺利解决.


---

* 位置: Buaa.AIBot.Repository.Implement.QuestionRepository.UpdateQuestionAsync
* 级别: 高并发的特殊情况下, 发生数据丢失错误.
* 描述: 当更新问题信息的标签信息时, 若在更新`标签-问题`表时, `标签`表删除了某个被选中的标签时, 将会导致整个问题被删除.
* 原因: 上述情况发生时, 应当视为操作失败, 应当撤销之前的更改, 但是由于实现的错误, 导致问题被删除, 这是由于这一部分是拷贝InsertQuestionAsync的实现然后进行修改的, 后者的`撤销`自然就是删除.
* 解决情况: 经过分析, 虽然本函数需要更改`问题`表和`标签-问题`表两个表, 但是并不像InsertQuestionAsync那样需要先插入`问题`表以获得问题的qid, 而不得不分两步完成, UpdateQuestionAsync只需要一次数据库数据的提交即可完成. 最后顺利修复.


---

* 位置: Buaa.AIBot.Repository.Implement.UserRepository.InsertUserAsync
* 级别: 代码编写错误的特殊情况下, 错误调用本方法未能报告错误.
* 描述: 当调用方未初始化UserInfo.Auth时未能报告错误.
* 原因: 接口设计时, 为每个引用类型设置了null检查, 但是Auth对应的时枚举类, 故遗漏了检查.
* 解决情况: 添加了对当Auth为AuthLevel.None时的报错, 同时更新了Buaa.AIBot.Repository.IUserRepository.InsertUserAsync中的描述. 顺利解决.


---

* 位置: Buaa.AIBot.Repository.Implement.AnswerRepository.InsertAnswerAsync
* 级别: 特殊情况下, 发生一致性错误, 导致语义错误.
* 描述: 创建新的回答时, 未检查创建者CreaterId的合法性, 导致CreaterId未对应任何用户时, 仍能够插入成功.
* 原因: 由于数据模型设计时, 为了实现`用户删除后其回答不会被`的功能, 未添加对CreaterId的强制约束, 但是语义上, 创建问题时应当保证用户存在.
* 解决情况: 添加了对当CreaterId为null时的报错, 同时更新了Buaa.AIBot.Repository.IAnswerRepository.InsertAnswerAsync中的描述. 顺利解决.


---

* 位置: Buaa.AIBot.Services.QuestionService.ModifyAnswerAsync
* 级别: 一般情况下, 后端报错.
* 描述: 调用该方法时, 即使待修改的回答时存在的, 仍旧会报错回答不存在.
* 原因: 调用Buaa.AIBot.Repository.Implement.AnswerRepository.UpdateAnswerAsync方法时, 少传了AnswerInfo.AnswerId参数.
* 解决情况: 顺利解决.


---

* 位置: Buaa.AIBot.Repository.Implement.QuestionRepository.SelectQuestionsByTagsAsync
* 级别: 一般情况下, 后端报错.
* 描述: 无论如何, 总会报错. 导致/api/questions/questionlist总是报错.
* 原因: `EF Core`对LINQ中GroupBy和Where支持有限, 导致原代码构建的表达式树无法转换为SQL语句, 从而导致了报错. 改为直接通过RawSql语句进行查询后, 由于开发环境为Windows, 生产/测试环境为Linux, 前者MySQL表名不区分大小写, 而后者区分, 导致过无法正确运行, 目前已解决该问题.
* 解决情况: 目前可以工作, 但是实现较为低下, 已有一定方案进行优化, 但是尚未进行.



---

* 位置: 
  * Buaa.AIBot.Repository.Implement.QuestionRepository.SelectAnswersForQuestionByIdAsync
  * Buaa.AIBot.Repository.Implement.QuestionRepository.SelectTagsForQuestionByIdAsync
  * Buaa.AIBot.Repository.Implement.UserRepository.SelectAnswersIdByIdAsync
  * Buaa.AIBot.Repository.Implement.UserRepository.SelectAnswersIdByIdByModifyTimeAsync
  * Buaa.AIBot.Repository.Implement.UserRepository.SelectQuestionsIdByIdAsync
  * Buaa.AIBot.Repository.Implement.UserRepository.SelectQuestionsIdByIdOrderByModifyTimeAsync
* 级别: 导致功能缺失.
* 描述: 无论是否创建了问题和回答, 是否为问题添加了相关标签, 都会导致通过用户搜索用户问答的问题和提出的问题, 以及通过问题搜索对应的回答和标签时, 将永远返回空列表. 导致以下接口无法正常工作(永远返回空列表, 或其中某部分为空列表):
  * /api/user/questions
  * /api/user/answers
  * /api/questions/question
* 原因: 错误地理解了`EF Core`对于一对多关系的加载策略, 误认为该关系是自动而主动由数据库加载到内存的, 事实上需要特定操作进行加载.
* 解决情况: 顺利解决.