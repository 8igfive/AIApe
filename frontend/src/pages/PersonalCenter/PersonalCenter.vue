<template>
    <el-container class="shell">
        <el-container class="list">
            <el-header>
                AIApe
            </el-header>
            <el-main class="selector">
                <el-button type="text" :class="{'unselected': select === 'answer'}" @click="handleSelect('question')">
                    我的提问
                </el-button>
                <el-button type="text" :class="{'unselected': select === 'question'}" @click="handleSelect('answer')">
                    我的回答
                </el-button>
            </el-main>
            <div style="height: auto; overflow: auto; width: 51vw" ref="scroll-body" id="scroll-body">
                <el-main class="question-list" v-if="select==='question'">
                    <div class="question-body" v-for="question in questions">
                        <i class="el-icon-delete" v-show="authority > 1 || currentUid === question.creatorId"
                           @click="deleteQuestion(question)"></i>
                        <el-link class='title' @click="goToDetail(question.id)" :underline="false">
                            {{ question.title }}
                        </el-link>
                        <div class="content">
                            {{ question.content }}
                        </div>
                        <div class="other-info">
                            <div class="tags">
                                <el-tag v-for="(tid, tName) in question.tags" :key="tid">{{ tName }}</el-tag>
                            </div>
                            <div class="recommend-time">
                                <el-button class="recommend" type="text"
                                           :icon="question.like? 'el-icon-star-on' : 'el-icon-star-off'"
                                           @click="like(question)">
                                    推荐{{ question.likeNum }}
                                </el-button>
                                <span>{{ question.date }}</span>
                            </div>
                        </div>
                    </div>
                </el-main>
                <el-main class="question-list" v-else>
                    <div class="question-body" v-for="answer in answers">
                        <i class="el-icon-delete" v-show="authority > 1 || currentUid === answer.creatorId"
                           @click="deleteAnswer(answer)"></i>
                        <el-link class='title' @click="goToDetail(answer.questionId)" :underline="false">
                            {{ answer.title }}
                        </el-link>
                        <div class="content">
                            {{ answer.content }}
                        </div>
                        <div class="other-info">
                            <div class="tags">
                                <el-tag v-for="(tid, tName) in answer.tags" :key="tid">{{ tName }}</el-tag>
                            </div>
                            <div class="recommend-time">
                                <el-button class="recommend" type="text"
                                           :icon="answer.like? 'el-icon-star-on' : 'el-icon-star-off'"
                                           @click="likeAnswer(answer)">
                                    推荐{{ answer.likeNum }}
                                </el-button>
                                <span>{{ answer.createTime }}</span>
                            </div>
                        </div>
                    </div>
                </el-main>
            </div>
        </el-container>
        <el-aside width="30vw">
            <el-main class="user-info">
                <div class="name-avatar">
                    {{ this.$store.state.username }}
                    <el-popover
                        placement="bottom-start"
                        width="175"
                        trigger="click">
                        <div>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar1.png" style="cursor: pointer" @click.native="changeAvatar(1)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar2.png" style="cursor: pointer" @click.native="changeAvatar(2)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar3.png" style="cursor: pointer" @click.native="changeAvatar(3)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar4.png" style="cursor: pointer" @click.native="changeAvatar(4)"></el-avatar>
                        </div>
                        <div>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar5.png" style="cursor: pointer" @click.native="changeAvatar(5)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar6.png" style="cursor: pointer" @click.native="changeAvatar(6)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar7.png" style="cursor: pointer" @click.native="changeAvatar(7)"></el-avatar>
                            <el-avatar src="http://81.70.211.128/aiape/icon-avatar8.png" style="cursor: pointer" @click.native="changeAvatar(8)"></el-avatar>
                        </div>
                        <el-avatar :src="avatarSrc"
                                   :size="80" slot="reference" style="cursor: pointer"></el-avatar>
                    </el-popover>
                </div>
                {{ email }}
            </el-main>
            <el-main class="info">
                <span>AIApe</span>
                <span>京ICP备 2021007509号-1</span>
                <span>
                联系我们 @2021软件工程DQSJ
            </span>
            </el-main>
        </el-aside>
    </el-container>
</template>

<script>
import store from "../../vuex/store";
import marked from 'marked';

export default {
    data() {
        return {
            select: 'question',
            questions: [],
            answers: [],
            question: '',
            email: '',
            uid: 0,
            likeValid: true,
            getQuestionValid: true,
            getAnswerValid: true
        }
    },
    methods: {
        changeAvatar(index) {
            let _this = this;
            this.$axios.put(this.BASE_URL + '/api/user/modify', {
                profilePhoto: index,
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
            .then((response) => {
                _this.$message({
                    message: '头像更换成功!',
                    type: 'success'
                });
                this.$store.state.avatarIndex = index;
            })
            .catch((error) => {
                console.log(error);
                _this.$message({
                    message: '头像更换失败!',
                    type: 'error'
                })
            })
        },
        handleSelect(selector) {
            this.$refs['scroll-body'].scrollTop = 0;
            this.select = selector;
            if (selector === 'answer') {
                this.getAnswers();
            } else if (selector === 'question') {
                this.getQuestions();
            }
        },
        goToDetail(qid) {
            this.$store.commit('setQuestionID', qid);
            this.$changePage(3);
        },
        async like(question) {
            if (!this.likeValid) {
                this.$message({
                    message: '操作过于频繁!',
                    type: 'warning'
                });
            }
            this.likeValid = false;
            let _this = this;
            let qid = question.id;
            let markAsLike = !question.like;
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
                    question.like = response.data.like;
                    question.likeNum = response.data.likeNum;
                })
                .catch(function (error) {
                    _this.$message({
                        message: '登录后才可以点赞~!',
                        type: 'warning'
                    })
                })
            this.likeValid = true;
        },
        deleteQuestion(question) {
            let _this = this;
            this.$confirm('此操作将永久删除该问题, 是否继续?', '删除确认', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                this.$axios.delete(this.BASE_URL + '/api/questions/delete_question', {
                    data: {
                        qid: question.id,
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
                        this.getQuestions();
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
        async getQuestions() {
            if (!this.getQuestionValid) {
                return;
            }

            this.getQuestionValid = false;
            let _this = this;

            await this.$axios.get(this.BASE_URL + '/api/user/questions', {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    let questionList = response.data.questions;
                    _this.questions = [];
                    for (let qid of questionList) {
                        await _this.$axios.get(_this.BASE_URL + '/api/questions/question?qid=' + qid, {
                            headers: {
                                Authorization: 'Bearer ' + _this.$store.state.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(async function (response) {
                                let question = {
                                    id: qid,
                                    title: response.data.question.title,
                                    content: marked(response.data.question.remarks).replace(/<[^>]+>/g, ""),
                                    tags: response.data.question.tags,
                                    date: response.data.question.createTime,
                                    likeNum: response.data.question.likeNum,
                                    like: response.data.question.like,
                                    creatorId: response.data.question.creator
                                };
                                let uid = response.data.question.creator;
                                await _this.$axios.get(_this.BASE_URL + '/api/user/public_info?uid=' + uid)
                                    .then(function (response) {
                                        question.creator = response.data.name;
                                    })
                                _this.questions.push(question);
                            });
                    }
                })
            this.getQuestionValid = true;
        },
        async getAnswers() {
            if (!this.getAnswerValid) {
                return;
            }

            this.getQuestionValid = false;
            let _this = this;
            await this.$axios.get(this.BASE_URL + '/api/user/answers', {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    _this.answers = [];
                    let QAList = response.data.answers;
                    for (let qa of QAList) {
                        let answer = {
                            questionId: qa.qid,
                            answerId: qa.aid
                        };
                        await _this.$axios.get(_this.BASE_URL + '/api/questions/question?qid=' + qa.qid, {
                            headers: {
                                Authorization: 'Bearer ' + _this.$store.state.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(function (response) {
                                answer.title = response.data.question.title;
                                answer.tags = response.data.question.tags;
                            })
                        await _this.$axios.get(_this.BASE_URL + '/api/questions/answer?aid=' + qa.aid, {
                            headers: {
                                Authorization: 'Bearer ' + _this.$store.state.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(function (response) {
                                answer.content = response.data.answer.content;
                                answer.createTime = response.data.answer.createTime;
                                answer.likeNum = response.data.answer.likeNum;
                                answer.like = response.data.answer.like;
                                answer.creatorId = response.data.answer.creator;
                            })
                        _this.answers.push(answer);
                    }
                })
            this.getAnswerValid = true;
        },
        async likeAnswer(answer) {
            if (!this.likeValid) {
                this.$message({
                    message: '操作过于频繁!',
                    type: 'warning'
                });
            }
            this.likeValid = false;
            let _this = this;
            let aid = answer.answerId;
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
            this.likeValid = true;
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
                        aid: answer.answerId
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
                        this.getQuestions();
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
    created() {
        let _this = this;
        this.getQuestions();
        this.getAnswers();
        this.$axios.get(this.BASE_URL + '/api/user/internal_info', {
            headers: {
                Authorization: 'Bearer ' + _this.$store.state.token,
                type: 'application/json;charset=utf-8'
            }
        })
            .then(function (response) {
                _this.email = response.data.email;
                _this.uid = response.data.uid;
            })
    },
    computed: {
        authority() {
            return this.$store.state.auth;
        },
        currentUid() {
            return this.$store.state.uid;
        },
        avatarSrc() {
            return 'http://81.70.211.128/aiape/icon-avatar' + this.$store.state.avatarIndex + '.png'
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
    background-color: rgba(246, 246, 246, 0.5);
}

.list {
    flex-direction: column;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    border-radius: 2px;
    height: 95vh;
    background-color: white;
    margin-right: 5px;
}

.el-header {
    padding-top: 10px;
    font-size: 30px;
}

.selector {
    flex: none;
    align-self: stretch;
    padding-left: 10px;
    border-bottom: 1px solid lightgrey;
}

.unselected {
    color: black;
}

.el-button {
    font-size: 20px;
}

.el-button:hover {
    color: #409eff;
}

.recommand:hover {
    color: rgb(39, 214, 214);
}

.question-list {
    align-self: stretch;
    flex-direction: column;
}

.user {
    align-self: flex-start;
    flex-direction: row;
    align-items: center;
}

.el-link {
    justify-content: flex-start;
    font-size: 20px;
    font-weight: bold;
    color: black;
    flex-grow: 0;
}

.question-list * {
    display: flex;
}

.question-body {
    flex-direction: column;
    padding: 10px;
    flex: 1 0 110px;
    border-bottom: 1px solid lightgrey;
}

.content {
    flex-grow: 2;
    justify-content: flex-start;
    align-items: flex-start;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    line-height: 20px;
    height: 60px;
    margin: 10px 15px;
}

.el-icon-delete {
    display: flex;
    align-self: flex-end;
    color: #F56C6C;
    cursor: pointer;
}

.title {
    font-size: 20px;
    font-weight: bold;
    margin-left: 10px;
    /*white-space: nowrap;*/
    /*overflow: hidden;*/
    /*text-overflow: ellipsis;*/
    /*display: block;*/
    margin-right: 20px;
}

.tags {
    flex-direction: row;
}

.other-info {
    justify-content: space-between;
    align-items: center;
    margin-left: 10px;
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

.recommend {
    height: 20px;
    font-size: 10px;
    line-height: 20px;
    padding: 3px 3px;
    margin-right: 20px;
    align-items: center;
}

.detail {
    text-overflow: ellipsis;
}

.el-aside {
    flex-direction: column;
    height: 95vh;
    overflow: visible;
    justify-content: space-between;
}

.user-info {
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    background-color: white;
    border-radius: 2px;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    flex-grow: 0;
    padding: 30px;
    margin-left: 10px;
}

.search-question {
    flex: 0 0 auto;
    box-shadow: 0 0 0 0;
    border-radius: 4px;
    margin-top: 0;
    margin-left: 10px;
    margin-right: 10px;
}

.info {
    box-shadow: 0 0 0 0;
    background-color: rgba(0, 0, 0, 0);
    flex-direction: column;
    align-items: center;
    flex-grow: 0;
    margin-top: 10px;
}

.name-avatar {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    font-size: 25px;
    align-self: stretch;
    margin-bottom: 20px;
}

.el-popover {
    display: flex;
    justify-content: space-between;
}
</style>
