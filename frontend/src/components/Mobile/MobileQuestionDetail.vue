<template>
    <el-container>
        <el-header height=5vh>
            AIApe
        </el-header>
        <el-header height=5vh class="back">
            <el-link v-on:click="goBack">返回</el-link>
        </el-header>
        <!--        <h1>问题详情页面{{ this.$store.state.questionID }}</h1>-->
        <div style="position:fixed; top: 10vh">
            <el-container class="list">
                <el-main class="question-detail">
                    <div class="user">
                        <el-avatar src="https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png"
                                   size="small" style="margin-right: 10px"></el-avatar>
                        {{ creatorName }}
                    </div>
                    <h1>{{ title }}</h1>
                    <div v-if="this.status==='question'">
                        <mavon-editor class="question" v-model="detail" ref=md
                                      :subfield="false" defaultOpen="preview"
                                      :toolbarsFlag="false" :editable="false"
                                      :scrollStyle="false" :box-shadow="false">
                        </mavon-editor>
                        <div class="other-info">
                            <div class="tags">
                                <el-tag v-for="(tid, tName) in tags" :key="tid">{{ tName }}</el-tag>
                            </div>
                            <div class="recommend-time">
                                <el-button style="margin-right: 5vw" icon="el-icon-edit-outline" size="mini"
                                           @click="answerAreaMove">我要回答
                                </el-button>
                                <el-button class="recommend" type="text"
                                           :icon="like? 'el-icon-star-on' : 'el-icon-star-off'"
                                           @click="like_question()">
                                    推荐{{ likeNum }}
                                </el-button>
                                <span>{{ date }}</span>
                            </div>
                        </div>
                    </div>
                </el-main>
                <el-main class="answers" v-if="this.status==='answers'">
                    <div v-for="answer in answers">
                        <div class="user">
                            <div>
                                <el-avatar src="https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png"
                                           size="small" style="margin-right: 11px"></el-avatar>
                                {{ answer.creatorName }}
                            </div>
                            <i style="margin-left: 90vw" class="el-icon-delete"
                               v-show="authority > 1 || currentUid === answer.creator"
                               @click="deleteAnswer(answer)"></i>
                        </div>
                        <mavon-editor ref=md v-model="answer.content"
                                      :subfield="false" defaultOpen="preview"
                                      :toolbarsFlag="false" :editable="false"
                                      :scrollStyle="false" :box-shadow="false">
                        </mavon-editor>
                        <div style="display: flex; justify-content: flex-end; align-items: center">
                            <el-button class="recommend" type="text"
                                       :icon="answer.like? 'el-icon-star-on' : 'el-icon-star-off'"
                                       @click="like_answer(answer)">
                                推荐{{ answer.likeNum }}
                            </el-button>
                            <span>{{ answer.createTime }}</span>
                        </div>
                    </div>
                    <div>
                        <h1><br><br><br><br><br><br><br><br></h1>
                    </div>
                </el-main>
                <el-link style="margin-top: 2vh;" :underline="false" v-if="this.status==='question'"
                         v-on:click="showAnswers">
                    显示回答
                </el-link>
                <el-main v-if="this.status === 'edit'">
                    <el-container style="display: flex; flex-direction: column">
                        <!--                        <mavon-editor :toolbars="toolbars" v-model="myAnswer" ref=md-->
                        <!--                                      :subfield="prop.subfield" :defaultOpen="prop.defaultOpen"-->
                        <!--                                      :toolbarsFlag="prop.toolbarsFlag" :editable="prop.editable"-->
                        <!--                                      :scrollStyle="prop.scrollStyle" :boxShadow="prop.boxShadow"-->
                        <!--                                      style="max-height: 0"-->
                        <!--                                      placeholder="编辑你的回答...">-->
                        <!--                        </mavon-editor>-->
                        <el-input
                            style="margin: 10vw 3vw 3vw 3vw"
                            type="textarea"
                            :rows="5"
                            placeholder="请输入内容"
                            v-model="myAnswer">
                        </el-input>
                        <el-button @click="submitAnswer">提交答案</el-button>
                    </el-container>
                </el-main>
            </el-container>
        </div>
    </el-container>
</template>

<script>
export default {
    name: "MobileQuestionDetail",
    data() {
        return {
            status: 'question',
            title: '',
            detail: '',
            tags: [],
            creatorName: '',
            date: '',
            answers: [],
            myAnswer: '',
            like: false,
            likeNum: 0,
            showAnswerArea: false,
            toolbars: {
                bold: true, // 粗体
                italic: true, // 斜体
                header: true, // 标题
                underline: true, // 下划线
                strikethrough: false, // 中划线
                mark: false, // 标记
                superscript: true, // 上角标
                subscript: true, // 下角标
                quote: false, // 引用
                ol: true, // 有序列表
                ul: true, // 无序列表
                link: false, // 链接
                imagelink: false, // 图片链接
                code: false, // code
                table: false, // 表格
                fullscreen: true, // 全屏编辑
                readmodel: false, // 沉浸式阅读
                htmlcode: false, // 展示html源码
                help: true, // 帮助
                /* 1.3.5 */
                undo: true, // 上一步
                redo: true, // 下一步
                trash: true, // 清空
                save: false, // 保存（触发events中的save事件）
                /* 1.4.2 */
                navigation: false, // 导航目录
                /* 2.1.8 */
                alignleft: true, // 左对齐
                aligncenter: true, // 居中
                alignright: true, // 右对齐
                /* 2.2.1 */
                subfield: false, // 单双栏模式
                preview: true // 预览
            },
            showAllState: false,
            maxHeight: '15vh'
        }
    },
    methods: {
        goBack() {
            if (this.$data.status !== 'question') {
                this.$data.status = 'question';
            } else {
                this.$store.state.mobileStatus = 'questionList';
            }
        },
        deleteAnswer(answer) {
            let _this = this;
            this.$confirm('此操作将永久删除该回答, 是否继续?', '删除确认', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                this.$axios.delete(this.BASE_URL + '/api/questions/delete_answer', {
                    data: {
                        aid: answer.id,
                    },
                    headers: {
                        Authorization: 'Bearer ' + _this.$store.state.token,
                        type: 'application/json;charset=utf-8'
                    }
                })
                    .then(response => {
                        this.$message({
                            type: 'success',
                            message: '删除成功!'
                        });
                        this.getQuestionDetail();
                    })
                    .catch(error => {
                        this.$message({
                            type: 'error',
                            message: '删除失败!'
                        });
                    })
            }).catch(() => {
                this.$message({
                    type: 'info',
                    message: '已取消删除'
                });
            })
        },
        getQuestionDetail() {
            let _this = this;
            let id = this.$store.state.questionID;
            _this.$axios.get(_this.BASE_URL + "/api/questions/question?qid=" + id, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    _this.title = response.data.question.title;
                    _this.detail = response.data.question.remarks;
                    let creatorId = response.data.question.creator;
                    _this.creatorId = creatorId;
                    await _this.$axios.get(_this.BASE_URL + '/api/user/public_info?uid=' + creatorId)
                        .then(async function (response) {
                            _this.creatorName = response.data.name;
                        })
                    _this.$data.date = response.data.question.createTime;
                    _this.$data.tags = response.data.question.tags;
                    _this.like = response.data.question.like;
                    _this.likeNum = response.data.question.likeNum;
                    let aidList = response.data.question.answers;
                    _this.answers = [];
                    for (let aid of aidList) {
                        await _this.$axios.get(_this.BASE_URL + "/api/questions/answer?aid=" + aid)
                            .then(async function (response) {
                                let answer = response.data.answer;
                                answer.id = aid;
                                let id = response.data.answer.creator;
                                if (id === _this.$store.state.uid) {
                                    _this.myAnswer = answer.content;
                                    _this.myAnswerId = aid;
                                }
                                await _this.$axios.get(_this.BASE_URL + '/api/user/public_info?uid=' + id)
                                    .then(function (response) {
                                        answer.creatorName = response.data.name;
                                    })
                                _this.answers.push(answer);
                            })
                            .catch(function (error) {
                                console.log(error);
                            })
                    }
                })
                .catch(function (error) {
                    console.log(error);
                });
        },
        answerAreaMove() {
            // if (!this.$data.showAnswerArea) {
            //     this.$data.showAnswerArea = true;
            //     this.$data.icon = 'el-icon-arrow-up';
            // } else {
            //     this.$data.showAnswerArea = false;
            //     this.$data.icon = 'el-icon-edit';
            // }
            // alert('herre!')
            this.$data.status = 'edit';
        },
        submitAnswer() {
            let answer = this.myAnswer;
            let _this = this;
            this.$axios.post(_this.BASE_URL + '/api/questions/add_answer', {
                qid: _this.$store.state.questionID,
                content: answer
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(function (response) {
                    _this.myAnswer = '';
                    if (response.data.status === 'success') {
                        _this.$message({
                            message: '感谢你的回答！',
                            type: 'success'
                        })
                        _this.getQuestionDetail();
                        _this.status = 'answers';
                    } else {
                        _this.$message({
                            message: '你已经回答过这个问题了！',
                            type: 'warning'
                        })
                    }
                })
        },
        async getUserName(uid) {
            let _this = this;
            let name = '';
            await this.$axios.get(this.BASE_URL + '/api/user/public_info?uid=' + uid)
                .then(function (response) {
                    name = response.data.name;
                })
            return name;
        },
        showAll() {
            if (!this.showAllState) {
                this.showAllState = true;
                this.maxHeight = '55vh';
            } else {
                this.showAllState = false;
                this.maxHeight = '15vh';
            }
        },
        showAnswers() {
            this.$data.status = 'answers';
        },
        like_question() {
            let _this = this;
            let qid = this.$store.state.questionID;
            let markAsLike = !this.like;
            this.$axios.post(this.BASE_URL + '/api/questions/like_question', {
                qid: qid,
                markAsLike: markAsLike
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    _this.like = response.data.like;
                    _this.likeNum = response.data.likeNum;
                })
                .catch(function (error) {
                    _this.$message({
                        message: '登录后才可以点赞~!',
                        type: 'warning'
                    })
                })
        },
        like_answer(answer) {
            let _this = this;
            let aid = answer.id;
            let markAsLike = !answer.like;
            this.$axios.post(this.BASE_URL + '/api/questions/like_answer', {
                aid: aid,
                markAsLike: markAsLike
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    answer.like = response.data.like;
                    answer.likeNum = response.data.likeNum;
                })
                .catch(function (error) {
                    _this.$message({
                        message: '登录后才可以点赞~!',
                        type: 'warning'
                    })
                })
        }
    },
    mounted() {
        this.getQuestionDetail();
    },
    computed: {
        prop() {
            return {
                subfield: false,// 单双栏模式
                defaultOpen: 'edit',//edit： 默认展示编辑区域 ， preview： 默认展示预览区域
                editable: true,
                toolbarsFlag: true,
                scrollStyle: false,
                boxShadow: true//边框
            };
        },
        currentUid() {
            return this.$store.state.uid;
        },
        authority() {
            return this.$store.state.auth;
        },
        questionId() {
            return this.$store.state.questionID;
        }
    },
}
</script>

<style scoped>

.user {
    align-self: stretch;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
}

.el-icon-delete {
    color: #F56C6C;
    cursor: pointer;
    font-size: 2vh;
}

.el-header {
    position: fixed;
    border-bottom: 1px solid #eaecf1;
    width: 100vw;
    align-items: center;
    padding: 5%;
    /*height: 5vh;*/
}

.back {
    position: fixed;
    top: 5vh;
    border-bottom: 0;
    width: 100vw;
    align-items: center;
    padding: 5%;
}

.list {
    width: 100vw;
    flex-direction: column;
    border-radius: 2px;
    height: 95vh;
    align-items: stretch;
    margin-right: 0px;
    overflow: visible;
}

.question-detail {
    flex-grow: 0;
    background-color: white;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    align-items: stretch;
    flex-direction: column;
    padding: 2vw;
    flex-shrink: 0;
}

.question {
    height: 30vh;
    min-height: 0vh;
    border: 0;
}

.answers {
    margin-top: 0;
    background-color: white;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    flex-direction: column;
    height: 0;
    flex-grow: 1;
    z-index: 1;
}

.answer {
    border-bottom: 1px solid lightgrey;
    padding: 10px;
}

.content {
    height: 10vh;
}

</style>
