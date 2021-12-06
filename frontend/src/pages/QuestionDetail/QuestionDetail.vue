<template>
    <el-container class="shell">
        <el-container class="list">
            <el-main class="question-detail">
                <div v-show="questionState === 'preview'" class="user">
                    <div style="display: flex; align-items: center">
                        <el-avatar :src="'http://81.70.211.128/aiape/icon-avatar' + avatarIndex + '.png'"
                                   size="small" style="margin-right: 10px"></el-avatar>
                        {{ creatorName }}
                    </div>
                    <i class="el-icon-delete"
                       v-show="this.$store.state.auth > 1 || currentUid === creatorId" @click="deleteQuestion"></i>
                </div>
                <h1 v-show="questionState === 'preview'">{{ title }}</h1>
                <el-input v-model="title" v-show="questionState === 'edit'"></el-input>
                <mavon-editor :style='"max-height:" + maxHeight' class="question" v-model="detail" ref=md
                              :subfield="false" :toolbars="toolbars"
                              :toolbarsFlag="questionState === 'edit'" :editable="questionState === 'edit'"
                              :scrollStyle="false" :box-shadow="false" :defaultOpen="questionState">
                </mavon-editor>
                <el-button style="align-self: flex-end; margin-top: 10px" v-if="questionState === 'edit'"
                           @click="modifyQuestion">提交修改
                </el-button>
                <el-button style="padding-bottom: 0" type="text" class="show-all" icon="el-icon-arrow-down"
                           @click="showAll" v-if="showAllState === false && questionState === 'preview'">展开
                </el-button>
                <el-button style="padding-bottom: 0" type="text" class="show-all" icon="el-icon-arrow-up"
                           @click="showAll" v-if="showAllState === true && questionState === 'preview'">收起
                </el-button>
                <div class="other-info">
                    <div class="tags">
                        <el-tag v-for="(tid, tName) in tags" :key="tid">{{ tName }}</el-tag>
                    </div>
                    <div class="recommend-time">
                        <el-button style="margin-right: 10px;" icon="el-icon-edit" size="mini" circle
                                   v-if="this.$store.state.uid === creatorId" @click="editQuestion">
                        </el-button>
                        <el-button style="margin-right: 10px" :icon="icon" size="mini"
                                   @click="answerAreaMove">我要回答
                        </el-button>
                        <el-button class="recommend"
                                   :icon="like? 'el-icon-star-on' : 'el-icon-star-off'"
                                   @click="like_question()" size="mini" type="text">
                            推荐{{ likeNum }}
                        </el-button>
                        <span>{{ date }}</span>
                    </div>
                </div>
            </el-main>
            <el-collapse-transition>
                <div v-show="showAnswerArea" style="display: flex; flex-direction: column; flex-shrink: 0">
                    <mavon-editor class="editor" :toolbars="toolbars" v-model="myAnswer" ref=md
                                  :subfield="prop.subfield" :defaultOpen="prop.defaultOpen"
                                  :toolbarsFlag="prop.toolbarsFlag" :editable="prop.editable"
                                  :scrollStyle="prop.scrollStyle" :boxShadow="prop.boxShadow"
                                  style="max-height: 0; z-index: 20"
                                  placeholder="编辑你的回答...">
                    </mavon-editor>
                    <el-button @click="submitAnswer" class="submit">提交答案</el-button>
                </div>
            </el-collapse-transition>
            <el-main class="answers">
                <div class="answer" v-for="answer in answers">
                    <div class="user">
                        <div style="display: flex; align-items: center">
                            <el-avatar :src="'http://81.70.211.128/aiape/icon-avatar' + answer.avatarIndex + '.png'"
                                       size="small" style="margin-right: 10px"></el-avatar>
                            {{ answer.creatorName }}
                        </div>
                        <i class="el-icon-delete" v-show="authority > 1 || currentUid === answer.creator"
                           @click="deleteAnswer(answer)"></i>
                    </div>
                    <mavon-editor class="content" ref=md v-model="answer.content"
                                  :subfield="false" defaultOpen="preview"
                                  :toolbarsFlag="false" :editable="false"
                                  :scrollStyle="false" :box-shadow="false"
                                  style="z-index: 1">
                    </mavon-editor>
                    <div style="display: flex; justify-content: flex-end; align-items: center">
                        <el-button style="margin-right: 10px;" icon="el-icon-edit" size="mini" circle
                                   v-if="currentUid === answer.creator" @click="answerAreaMove">
                        </el-button>
                        <el-button class="recommend" type="text"
                                   :icon="answer.like? 'el-icon-star-on' : 'el-icon-star-off'"
                                   @click="like_answer(answer)">
                            推荐{{ answer.likeNum }}
                        </el-button>
                        <span>{{ answer.createTime }}</span>
                    </div>
                </div>
            </el-main>
        </el-container>
        <DetailSideBar/>
    </el-container>
</template>

<script>
import MarkdownItVue from 'markdown-it-vue'
import 'markdown-it-vue/dist/markdown-it-vue.css'
import DetailSideBar from "./DetailSideBar";

export default {
    components: {
        MarkdownItVue,
        DetailSideBar
    },
    metaInfo: {
        meta: [
            {name: 'referrer', content: 'no-referrer'},
        ]
    },
    data() {
        return {
            title: '',
            detail: '',
            tags: [],
            questionState: 'preview',
            creatorName: '',
            creatorId: -1,
            date: '',
            answers: [],
            like: false,
            likeNum: 0,
            showAnswerArea: false,
            avatarIndex: 0,
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
            minHeight: 0,
            maxHeight: '15vh',
            modifyAnswerId: -1,
            icon: 'el-icon-edit-outline',
            myAnswer: '',
            myAnswerId: -1
        }
    },
    methods: {
        goBack() {
            this.$router.replace('/questionList');
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
                            _this.avatarIndex = response.data.profilePhoto;
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
                                        answer.avatarIndex = response.data.profilePhoto;
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
            if (!this.$data.showAnswerArea) {
                this.$data.showAnswerArea = true;
                this.$data.icon = 'el-icon-arrow-up';
            } else {
                this.$data.showAnswerArea = false;
                this.$data.icon = 'el-icon-edit-outline';
            }
        },
        submitAnswer() {
            let answer = this.myAnswer;
            if (this.$store.state.username === '') {
                this.$message({
                    message: '登录后才能回答~！',
                    type: 'warning'
                })
                return;
            }
            if (this.myAnswer === '') {
                this.$message({
                    message: '回答不能为空！',
                    type: 'warning'
                })
                return;
            }
            let _this = this;
            if (this.myAnswerId > 0) {
                this.$confirm('你已经回答过这个问题，确认修改你的答案吗？', '修改确认', {
                    cancelButtonText: '取消',
                    confirmButtonText: '确定',
                })
                    .then(function (response) {
                        _this.$axios.put(_this.BASE_URL + '/api/questions/modify_answer', {
                            aid: _this.myAnswerId,
                            content: _this.myAnswer
                        }, {
                            headers: {
                                Authorization: 'Bearer ' + _this.$store.state.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(function (response) {
                                _this.$message({
                                    message: '修改成功！',
                                    type: 'success'
                                })
                                _this.getQuestionDetail();
                            })
                            .catch(function (error) {
                                _this.$message({
                                    message: '修改失败！',
                                    type: 'error'
                                })
                            })
                    })
                    .catch(function (error) {
                        _this.$message({
                            type: 'info',
                            message: '取消修改'
                        });
                    })
            } else {
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
                            location.reload();
                        }
                    })
            }
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
        },
        editQuestion() {
            this.questionState = 'edit';
            this.maxHeight = '55vh';
        },
        modifyQuestion() {
            let _this = this;
            this.$confirm('确认修改你的问题吗？', '修改确认', {
                cancelButtonText: '取消',
                confirmButtonText: '确定',
            })
                .then(function (response) {
                    _this.$axios.put(_this.BASE_URL + '/api/questions/modify_question', {
                        qid: _this.$store.state.questionID,
                        question: _this.title,
                        remarks: _this.detail
                    }, {
                        headers: {
                            Authorization: 'Bearer ' + _this.$store.state.token,
                            type: 'application/json;charset=utf-8'
                        }
                    })
                        .then(function (response) {
                            if (response.data.status === 'success') {
                                _this.$message({
                                    message: '修改成功',
                                    type: 'success'
                                });
                            }
                            _this.questionState = 'preview';
                            if (!_this.showAllState) {
                                _this.maxHeight = '15vh';
                            }
                        })
                        .catch(function (error) {
                            console.log(error);
                            _this.$message({
                                message: '修改失败',
                                type: 'error'
                            });
                        })
                })
                .catch(function (error) {
                    _this.$message({
                        type: 'info',
                        message: '取消修改'
                    });
                })
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
        deleteQuestion() {
            let _this = this;
            this.$confirm('此操作将永久删除该问题, 是否继续?', '删除确认', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                this.$axios.delete(this.BASE_URL + '/api/questions/delete_question', {
                    data: {
                        qid: _this.$store.state.questionID
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
                        this.$store.state.questionID = 0;
                        this.$changePage(2);
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
    watch: {
        currentUid() {
            for (let answer of this.answers) {
                if (answer.creator === this.currentUid) {
                    this.myAnswer = answer.content;
                    this.myAnswerId = answer.id;
                    break;
                }
            }
        },
        questionId() {
            this.getQuestionDetail();
        }
    }
}
</script>

<style scoped>
.shell {
    position: absolute;
    left: 5vw;
    top: 0;
    width: 95vw;
    height: 100vh;
    padding-left: 100px;
    padding-right: 100px;
    background-color: rgba(246, 246, 246, 1);
}

.list {
    flex-direction: column;
    border-radius: 2px;
    height: 95vh;
    align-items: stretch;
    margin-right: 5px;
    overflow: visible;
}

.el-header {
    padding-top: 5px;
    font-size: 30px;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    background-color: white;
    align-self: stretch;
    justify-content: center;
}

.question-detail {
    flex-grow: 0;
    background-color: white;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    align-items: stretch;
    flex-direction: column;
    padding: 10px;
    flex-shrink: 0;
}

.question-detail * {
    display: flex;
}

h1 {
    margin-left: 30px;
}

.question {
    min-height: 15vh;
    border: 0;
}

.v-show-content {
    background-color: white !important;
}

.other-info {
    display: flex;
    margin-top: 10px;
    flex-direction: row;
    justify-content: space-between;
}

.el-tag {
    margin-left: 10px;
    margin-top: 5px;
}

i {
    cursor: pointer;
    color: #409eff;
    font-size: 20px;
}

.recommend-user-time {
    display: flex;
    flex-direction: row;
}

/*.show-answer {*/
/*    font-size: 10px;*/
/*    height: 10px;*/
/*    width: 10px;*/
/*    line-height: 10px;*/
/*    margin-right: 10px;*/
/*    text-align: center;*/
/*}*/

.recommend {
    height: 20px;
    font-size: 10px;
    line-height: 10px;
    padding: 3px 3px;
    margin-right: 20px;
    align-items: center;
}


.user-time {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    margin-right: 10px;
}

.markdown-body {
    border-radius: 0;
}

.editor {
    margin-top: 10px;
}

.answers {
    margin-top: 10px;
    background-color: white;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    flex-direction: column;
}

.content {
    border: 0;
    min-height: 0;
}

.answer {
    border-bottom: 1px solid lightgrey;
    padding: 10px;
}

.answer-user-time {
    margin-bottom: 10px;
}

.user {
    display: flex;
    align-self: stretch;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
}

.other-info {
    justify-content: space-between;
    align-items: center;
    margin-left: 25px;
}

.recommend-time {
    flex-direction: row;
    align-items: center;
}

.el-tag {
    height: 25px;
    line-height: 23px;
    font-size: 12px;
    margin-left: 5px;
    margin-bottom: 5px;
}

.show-all {
    align-self: flex-end;
    color: black;
}

.show-all:hover {
    color: #409eff;
}

.fullscreen {
    min-height: 100vh;
    max-height: 100vh;
}

::-webkit-scrollbar {
    width: 0 !important;
}

.submit {
    margin-top: 10px;
    align-self: flex-end;
}

.el-icon-delete {
    display: flex;
    justify-self: flex-end;
    color: #F56C6C;
    cursor: pointer;
}
</style>
