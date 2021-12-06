+ 位置：

  Buaa.AIBot.Controllers.UserController.GetInternalInfoAsync

+ 级别：比较严重，导致API无法正常响应。

+ 描述：在请求用户内部信息的时候总是返回404NotFound。

+ 原因：在判断是否存在该用户时逻辑写反了。

+ 解决情况：修复了判断逻辑，现在能正常判断用户是否存在并获取其内部信息。

---

+ 位置：

  Buaa.AiBot.Controllers.UserController.FreshAsync

+ 级别：比较严重，导致无法更新令牌。

+ 描述：在JWT令牌未过期时请求更新令牌失败。

+ 原因：从请求中获取令牌的位置出现了错误，主要涉及到两个方法

  1. UserService.FreshTokenAsync
  2. UserService.GetExpirationFromToken

  这两个方法都错误地从请求参数中获取JWT令牌，使得令牌获取失败，进而导致无法更新令牌。

+ 解决情况：修复了令牌获取方式，现在令牌可以正确地从请求体中得到。

---

+ 位置：

  Buaa.AIBot.Controllers.QuestionsController.AddAnswerAsync

+ 级别：一般严重，使得后端在一定情况下会产生错误。

+ 描述：如果一个问题不存在的时候回答该问题会产生错误。

+ 原因：调用Buaa.AIbot.Services.IQuestionService.AddAnswerAsync是未接住其抛出的QuestionNotExistException。

+ 解决情况：成功解决，正确接住了并处理了其抛出的异常。

---

+ 位置：

  Buaa.AIBot.Controllers.UserController.SignUpAsync

+ 级别：不太严重，主要影响前端人员测试

+ 描述：无法创建管理员用户

+ 原因：在控制层实现signup api时，没有考虑到增加管理员用户的需求。

+ 解决情况：在signup逻辑中增加了添加管理员账户逻辑。

----

+ 位置：

  Buaa.AIBot.Repository.BaiduCrawlerRepository.onCompleted

+ 级别：不太严重，主要影响的时处理效率。

+ 描述：在使用正则表达式处理爬取的数据时效率较低。

+ 原因：错误地在一个循环中进行了多余的正则替换

+ 解决情况：将正则替换移到循环外，提高了处理效率。

---

