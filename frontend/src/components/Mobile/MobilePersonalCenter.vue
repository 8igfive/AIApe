<template>
    <el-container>
        <el-header height=5vh>
            AIApe
        </el-header>
        <el-main class="user-info" height="15vh">
            <div class="name-avatar">
                {{ this.$store.state.username }}
                <el-avatar src="https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png"
                           :size="80"></el-avatar>
            </div>
            <span style="padding-left: 5vw; padding-bottom: 2vh">{{ email }}</span>
        </el-main>
        <el-container class="list">
            <el-main class="selector">
                <el-button type="text" :class="{'unselected': select === 'answer'}" @click="handleSelect('question')">
                    我的提问
                </el-button>
                <el-button type="text" :class="{'unselected': select === 'question'}" @click="handleSelect('answer')">
                    我的回答
                </el-button>
            </el-main>

            <div style="height: auto; overflow: auto; width: 100vw" ref="scroll-body" id="scroll-body">
                <el-main class="question-list" v-if="select==='question'">
                    <div class="question-body" v-for="question in questions">
                        <div class="user">
                            <div style="width: 100vw">
                                <el-avatar src="https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png"
                                           size="small" style="margin-right: 10px"></el-avatar>
                                {{ question.creator }}
                                <i class="el-icon-delete"
                                   v-show="true"
                                   @click="deleteQuestion(question)"></i>
                            </div>
                        </div>
                        <el-link class='title' @click="goToDetail(question.id)" :underline="false">
                            {{ question.title }}
                        </el-link>
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
                        <i class="el-icon-delete"
                           v-show="true"
                           @click="deleteAnswer(answer)">
                        </i>
                        <br/>
                        <div>
                            <el-link class='title' @click="goToDetail(answer.questionId)" :underline="false">
                                {{ answer.title }}
                            </el-link>
                        </div>
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
    </el-container>
</template>

<script>
import marked from "marked";

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
            getAnswerValid: true,
        }
    },
    methods: {
        handleSelect(selector) {
            this.select = selector;
        },
        goToDetail(qid) {
            this.$store.commit('setQuestionID', qid);
            this.$store.state.mobileStatus = 'questionDetail';
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
        getQuestions() {
            let _this = this;
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
            this.$axios.get(this.BASE_URL + '/api/user/questions', {
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
                                    like: response.data.question.like
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
        }
    },
}
</script>

<style scoped>

.el-icon-delete {
    position: absolute;
    /*right: 10vw;*/
    left: 87vw;
    color: #F56C6C;
    cursor: pointer;
    font-size: 2vh;
    margin-top: 1.5vh;
    /*margin-left: 90vw;*/
}

.el-header {
    position: fixed;
    border-bottom: 1px solid #eaecf1;
    width: 100vw;
    align-items: center;
    padding: 5%;
    /*height: 5vh;*/
}

.list {
    position: fixed;
    top: 20vh;
    bottom: 5vh;
    flex-direction: column;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    border-radius: 2px;
    height: 75vh;
    width: 100vw;
    background-color: white;
    margin-right: 0;
}

.selector {
    width: 100vw;
    position: fixed;
    top: 20vh;
    flex: none;
    align-self: stretch;
    padding-left: 10px;
    border-bottom: 1px solid lightgrey;
}

.unselected {
    color: black;
}

.user-info {
    position: fixed;
    top: 5vh;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    background-color: white;
    border-radius: 2px;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    flex-grow: 0;
    /*padding: 30px;*/
    /*margin-left: 10px;*/
    width: 100vw;
}

.name-avatar {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    font-size: 25px;
    align-self: stretch;
    margin-bottom: 0vh;
    padding-left: 5vw;
    padding-right: 5vw;
    padding-top: 1vh;
}

.question-list {
    position: fixed;
    top: 25vh;
    bottom: 5vh;
    align-self: stretch;
    flex-direction: column;
}

.user {
    align-self: stretch;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
}

.question-body {
    flex-direction: column;
    padding: 10px;
    flex: 1 0 110px;
    border-bottom: 1px solid lightgrey;
}

.title {
    font-size: 20px;
    font-weight: bold;
    margin-top: 10px;
    margin-left: 5vw;
    /*white-space: nowrap;*/
    /*overflow: hidden;*/
    /*text-overflow: ellipsis;*/
    /*display: block;*/
    margin-right: 20px;
    overflow: hidden;
}

.content {
    margin-left: 5vw;
    margin-top: 1.5vh;
}

.tags {
    flex-direction: row;
}

.other-info {
    justify-content: space-between;
    align-items: center;
    margin-left: 25px;
    margin-right: 20px;
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
    background-color: rgb(255, 255, 255);
    border-color: rgb(255, 255, 255);
    color: #966dff;
    padding: 3px 3px;
    margin-right: 20px;
    align-items: center;
}
</style>
