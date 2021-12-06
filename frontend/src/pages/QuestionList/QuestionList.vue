<template>
    <el-container class="shell">
        <el-container class="list">
            <el-header>
                AIApe
            </el-header>
            <el-main class="selector">
                <el-button type="text" :class="{'unselected': select === 'hot'}" @click="handleSelect('new')">最新
                </el-button>
                <el-button type="text" :class="{'unselected': select === 'new'}" @click="handleSelect('hot')">热门
                </el-button>
            </el-main>
            <div style="height: auto; overflow: auto; width: 51vw" ref="scroll-body" id="scroll-body"
                 @scroll="loadMore">
                <el-main class="question-list" v-if="select==='new'">
                    <div class="question-body" v-for="question in questionList">
                        <div class="user">
                            <div style="display: flex; align-items: center">
                                <el-avatar :src="'http://81.70.211.128/aiape/icon-avatar' + question.avatarIndex + '.png'"
                                           size="small" style="margin-right: 10px"></el-avatar>
                                {{ question.creator }}
                                {{ question.avatarIndex }}
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
                <el-main class="question-list" v-else>
                    <div class="question-body" v-for="question in hots">
                        <div class="user">
                            <div style="display: flex; align-items: center">
                                <el-avatar :src="'http://81.70.211.128/aiape/icon-avatar' + question.avatarIndex + '.png'"
                                           size="small" style="margin-right: 10px"></el-avatar>
                                {{ question.creator }}
                            </div>
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
            </div>
        </el-container>
        <ListSideBar/>
    </el-container>
</template>

<script>
import ListSideBar from "./ListSideBar";
import 'markdown-it-vue/dist/markdown-it-vue.css';
import marked from 'marked';

export default {
    components: {
        ListSideBar
    },
    data() {
        return {
            hots: [],
            questionList: [],
            last_index: 0,
            showTag: true,
            isAdmin: false,
            showResearchInput: false,
            select: 'new',
            loading: false,
            no_more: false,
            likeValid: true,
            getHotValid: true,
            getQuestionsValid: true
        }
    },
    mounted() {
        this.initTagState();
        this.isAdmin = (this.$store.state.auth === 2);
    },
    created() {
        this.getQuestions();
    },
    methods: {
        handleSelect(selector) {
            this.$refs['scroll-body'].scrollTop = 0;
            this.select = selector;
            if (selector === 'hot') {
                this.getHot();
            }
            if (selector === 'new') {
                this.getQuestions(0, true);
            }
        },
        async getQuestions(pt, reset) {
            if (!this.getQuestionsValid) {
                return;
            }

            if (reset) {
                this.questionList = [];
            }

            this.getQuestionsValid = false;
            this.$store.state.questionList.tagSelectValid = false;
            let _this = this;

            let post_data = {
                number: 8,
                tags: this.selectedTag
            }
            if (pt > 0) {
                post_data['pt'] = pt;
            }
            await _this.$axios.post(_this.BASE_URL + '/api/questions/questionlist', post_data)
                .then(async function (response) {
                    let questionIdList = response.data;
                    _this.last_index = questionIdList[questionIdList.length - 1];
                    if (questionIdList.length < 8) {
                        _this.no_more = true;
                    }
                    for (let qid of questionIdList) {
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
                                        question.avatarIndex = response.data.profilePhoto;
                                    })
                                _this.questionList.push(question);
                            });
                    }
                })
                .catch(function (error) {
                });
            this.getQuestionsValid = true;
            this.$store.state.questionList.tagSelectValid = true;
        },
        gotoAdministration() {
            this.$router.replace('/administration');
        },
        handleCommand(command) {
            if (command === 'personalCenter') {
                this.goToPersonalCenter();
            } else if (command === 'administration') {
                this.gotoAdministration();
            }
        },
        initTagState() {
            let tagList = this.$store.state.tagList;
            for (let category in tagList) {
                for (let tag in tagList[category]) {
                    this.$store.state.questionList.tagState[tagList[category][tag]] = false;
                }
            }
        },
        async loadMore() {
            if (this.select !== 'new') {
                return;
            }
            let e = this.$refs['scroll-body'];
            if (e.scrollTop + e.offsetHeight > e.scrollHeight - 10 && !this.loading && !this.no_more) {

                this.loading = true;
                await this.getQuestions(this.last_index);
                this.loading = false;
            }
        },
        tagClick(tid) {
            if (!this.tagState[tid]) {
                this.tagState[tid] = true;
                this.selectedTag.push(tid);
            } else {
                this.tagState[tid] = false;
                let index = this.selectedTag.indexOf(tid);
                this.selectedTag.splice(index, 1);
            }
            this.showTag = false;
            this.$nextTick(function () {
                this.showTag = true;
            })
            this.getQuestions();
        },
        close() {
            this.$store.state.show.questionList = false;
        },
        gotoTop() {
            this.$store.state.maxZIndex += 1
            this.zIndex = this.$store.state.maxZIndex;
        },
        async getHot() {
            if (!this.getHotValid) {
                return;
            }
            this.hots = [];
            this.getHotValid = false;
            let _this = this;
            await this.$axios.get(_this.BASE_URL + '/api/questions/hotlist')
                .then(async function (response) {
                    let hotList = response.data;
                    for (let qid of hotList) {
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
                                        question.avatarIndex = response.data.profilePhoto;
                                    })
                                _this.hots.push(question);
                            })
                    }
                });
            this.getHotValid = true;
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
                        this.getQuestions(0, true);
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
        goToDetail(qid) {
            this.$store.commit('setQuestionID', qid);
            this.$changePage(3);
        },
    },
    computed: {
        username() {
            return this.$store.state.username;
        },
        selectedTag() {
            return this.$store.state.questionList.selectedTag;
        },
        currentUId() {
            return this.$store.state.uid;
        },
        authority() {
            return this.$store.state.auth;
        }
    },
    watch: {
        username: function () {
            let _this = this;
            for (let i = 0; i < _this.questionList.length - 1; i++) {
                let qid = _this.questionList[i].id;
                _this.$axios.get(_this.BASE_URL + '/api/questions/question?qid=' + qid, {
                    headers: {
                        Authorization: 'Bearer ' + _this.$store.state.token,
                        type: 'application/json;charset=utf-8'
                    }
                })
                    .then(function (response) {
                        _this.questionList[i].like = response.data.question.like;
                    })
            }
            for (let i = 0; i < _this.hots.length - 1; i++) {
                let qid = _this.hots[i].id;
                _this.$axios.get(_this.BASE_URL + '/api/questions/question?qid=' + qid, {
                    headers: {
                        Authorization: 'Bearer ' + _this.$store.state.token,
                        type: 'application/json;charset=utf-8'
                    }
                })
                    .then(function (response) {
                        _this.hots[i].like = response.data.question.like;
                    })
            }
        },
        selectedTag: function () {
            this.no_more = false;
            this.getQuestions(0, true);
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

.el-icon-search {
    color: #409eff;
    font-size: 30px;
}

.el-icon-search:hover {
    color: #6dfff3;
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
    flex-wrap: wrap;
}

.other-info {
    justify-content: space-between;
    align-items: center;
    margin-left: 25px;
}

.recommend-time {
    flex-direction: row;
    align-items: center;
    flex-shrink: 0;
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

.el-icon-delete {
    display: flex;
    justify-self: flex-end;
    color: #F56C6C;
    cursor: pointer;
}
</style>
