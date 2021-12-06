<template>
    <el-container class="shell">
        <el-container class="list">
            <el-header>
                AIApe
            </el-header>
            <el-main class="selector">
                <el-button type="text" style="cursor: default">搜索结果</el-button>
            </el-main>
            <el-main class="question-list">
                <div class="question-body" v-for="question in questions">
                    <div class="user">
                        <div style="display: flex; align-items: center">
                            <el-avatar :src="'http://81.70.211.128/aiape/icon-avatar' + question.avatarIndex + '.png'"
                                       size="small" style="margin-right: 10px"></el-avatar>
                            {{ question.creator }}
                        </div>
                        <i class="el-icon-delete"
                           v-show="authority > 1 || currentUId === question.creatorId"
                           @click="deleteQuestion(question)"></i>
                    </div>
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
        </el-container>
        <DetailSideBar/>
    </el-container>
</template>
<script>
import ResultSideBar from "./ResultSideBar";
import DetailSideBar from "../QuestionDetail/DetailSideBar";

export default {
    components: {
        DetailSideBar
    },
    data() {
        return {
            likeValid: true
        }
    },
    computed: {
        questions() {
            return this.$store.state.searchResult;
        },
        authority() {
            return this.$store.state.auth;
        },
        currentUId() {
          return this.$store.state.uid;
        }
    },
    methods: {
        goToDetail(qid) {
            this.$store.commit('setQuestionID', qid);
            this.$changePage(3);
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

.question-list {
    align-self: stretch;
    flex-direction: column;
}

.user {
    align-self: stretch;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
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
    margin: 10px 30px;
}

.title {
    font-size: 20px;
    font-weight: bold;
    margin-top: 10px;
    margin-left: 30px;
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

.recommend {
    height: 20px;
    font-size: 10px;
    line-height: 20px;
    background-color: rgb(255, 255, 255);
    border-color: rgb(255, 255, 255);
    padding: 3px 3px;
    margin-right: 20px;
    align-items: center;
}

.detail {
    text-overflow: ellipsis;
}

.el-icon-delete {
    display: flex;
    justify-self: flex-end;
    color: #F56C6C;
    cursor: pointer;
}
</style>
