<template>
    <el-container>
        <el-header height="5vh">
            AIApe
        </el-header>
        <el-main class="log">
            <div style="width: 60vw; overflow-y: scroll" ref="words" id="words">
                <div v-for="msg in this.$store.state.logs" class="content">
                    <div :class="msg.id === 1? 'user':'bot'">
                        <el-avatar
                            :src="msg.id === 1? 'http://81.70.211.128/aiape/icon-avatar' + avatarIndex + '.png': bot_avatar"
                            size="medium" style="background-color: #fff"></el-avatar>
                        <span class="chat-content" v-html="msg.content"></span>
                    </div>
                    <div class="prompts" v-show="msg.promptValid">
                        <el-button class="prompt" v-for="prompt in msg.prompts"
                                   :key="prompt" @click="choosePrompt(prompt, msg)">
                            {{ prompt }}
                        </el-button>
                    </div>
                </div>
            </div>
        </el-main>
        <el-main class="send-area">
            <div style="width: 60vw; height: 20vh">
                <el-input class="textarea" type="textarea" resize="none" v-model="message"></el-input>
                <el-button type="primary" @click="send">发送</el-button>
            </div>
        </el-main>
    </el-container>
</template>

<script>
import $ from 'jquery';

export default {
    data() {
        return {
            message: '',
            bot_avatar: require('../../assets/bot.png')
        }
    },
    computed: {
        username() {
            return this.$store.state.username;
        },
        logs() {
            return this.$store.state.logs;
        },
        avatarIndex() {
            return this.$store.state.avatarIndex;
        }
    },
    methods: {
        send() {
            let _this = this;
            let message = this.message;
            if (this.$store.state.username === '') {
                this.$store.commit('addAImessage', {id: 2, content: '你好，请先登录！', prompts: [], promptValid: false});
                return;
            }
            if (this.$data.message === '') {
                this.$message({
                    message: '消息不能为空!',
                    type: 'warning'
                })
                return;
            }

            for (let prompt of this.$store.state.logs[this.$store.state.logs.length - 1].prompts) {
                if (prompt === this.message) {
                    this.$store.state.logs[this.$store.state.logs.length - 1].promptValid = false;
                    break;
                }
            }

            message = this.return2Br(this.html2Escape(message));
            this.$store.state.logs[this.$store.state.logs.length - 1].promptValid = false;
            this.$store.commit('addUserMessage', message);

            this.$axios.post(this.BASE_URL + '/api/bot/message', {
                message: this.$data.message
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = await _this.transform(message);
                        let payload = {};
                        payload['content'] = message;
                        if (i === response.data.messages.length - 1) {
                            payload['prompts'] = response.data.prompt;
                            payload['promptValid'] = response.data.prompt.length > 0;
                        } else {
                            payload['prompts'] = [];
                            payload['promptValid'] = false;
                        }
                        i++;
                        _this.$store.commit('addAImessage', payload);
                        // if (message.indexOf('el-link') !== -1) {
                        //     location.reload();
                        // }
                        if (message.indexOf('小猿已经帮您创建好提问模板') !== -1) {
                            _this.$axios.get(_this.BASE_URL + '/api/bot/question_template', {
                                headers: {
                                    Authorization: 'Bearer ' + _this.$store.state.token,
                                    type: 'application/json;charset=utf-8'
                                }
                            })
                                .then((response) => {
                                    _this.$store.state.template = response.data.template;
                                    _this.$store.state.templateExist = true;
                                })
                        }
                    }
                })
                .catch(function (error) {
                })
            this.$data.message = '';

        },
        getTagList() {
            let _this = this;
            _this.$axios.get(_this.BASE_URL + '/api/questions/taglist')
                .then(function (response) {
                    let tagList = response.data;
                    _this.$store.commit('setTagList', tagList);
                })
        },
        async transform(msg) {
            let _this = this;
            msg = msg.replaceAll('\n', '<br/>');
            let left, right;
            left = msg.indexOf('[');
            right = msg.indexOf(']');
            while (left !== -1) {
                let space = msg.substring(left, right).indexOf(' ');
                let url;
                let type = 'url';
                let id;
                if (space !== -1) {
                    type = msg.substring(left + 1, left + space);
                    if (type === 'question') {
                        id = msg.substring(left + space + 1, right);
                    } else {
                        url = msg.substring(left + space + 1, right);
                    }
                } else {
                    url = msg.substring(left + 1, right);
                }
                if (type === 'question') {
                    let title = '';
                    await _this.$axios.get(_this.BASE_URL + '/api/questions/question?qid=' + id)
                        .then(function (response) {
                            title = response.data.question.title;
                        })
                    msg = msg.substring(0, left) +
                        '<a style="color: #409eff; cursor: pointer" onclick="gotoQuestionDetail(' + id + ')">' +
                        title + '</a>' + msg.substring(right + 1);
                } else {
                    msg = msg.substring(0, left) + '<a style="color: #409eff" href="' + url + '" target="_blank">' + url + '</a>' + msg.substring(right + 1);
                }
                left = msg.indexOf('[');
                right = msg.indexOf(']');
            }
            return msg;
        },
        choosePrompt(prompt, msg) {
            msg.promptValid = false;
            this.$store.commit("addUserMessage", prompt);
            let _this = this;
            this.$axios.post(this.BASE_URL + '/api/bot/message', {
                message: prompt
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = await _this.transform(message);
                        let payload = {};
                        payload['content'] = message;
                        if (i === response.data.messages.length - 1) {
                            payload['prompts'] = response.data.prompt;
                            payload['promptValid'] = response.data.prompt.length > 0;
                        } else {
                            payload['prompts'] = [];
                            payload['promptValid'] = false;
                        }
                        i++;
                        _this.$store.commit('addAImessage', payload);
                    }
                })
                .catch(function (error) {
                })
        },
        gotoQuestionDetail(id) {
            this.$store.state.questionID = id;
            this.$changePage(3);
        },
        html2Escape(sHtml) {
            return sHtml.replace(/[<>&"]/g, function (c) {
                return {'<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;'}[c];
            });
        },
        return2Br(str) {
            return str.replace(/\r?\n/g, "<br />");
        }
    },
    watch: {
        username: function (username) {
            if (username === '') {
                this.$store.state.logs = [{id: 2, content: '你好，我是AIApe！请先登录！', prompts: [], promptValid: false}];
                return;
            }
            let _this = this;
            this.waitTag = true;
            this.$store.commit('addAImessage', {
                id: 2,
                content: '你好，' + username + '！',
                prompts: [],
                promptValid: false
            });
            _this.$axios.post(_this.BASE_URL + '/api/bot/start', {}, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(async function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = await _this.transform(message);
                        let payload = {};
                        payload['content'] = message;
                        if (i === response.data.messages.length - 1) {
                            payload['prompts'] = response.data.prompt;
                            payload['promptValid'] = response.data.prompt.length > 0;
                        } else {
                            payload['prompts'] = [];
                            payload['promptValid'] = false;
                        }
                        i++;
                        _this.$store.commit('addAImessage', payload);
                    }
                })
        },
        logs: function () {
            this.$nextTick(() => {
                $('#words').animate({scrollTop: $('#words')[0].scrollHeight}, 800);
            })
        }
    },
    mounted() {
    },
    created() {
        window.gotoQuestionDetail = this.gotoQuestionDetail;
    }
}
</script>

<style scoped>

.el-container {
    box-shadow: 0 2px 4px rgba(0, 0, 0, .12), 0 0 6px rgba(0, 0, 0, .04);
    background-color: white;
    flex: 0 0 60vw;
    height: 80vh;
    margin-left: 15vw;
    border-radius: 5px;
    align-items: stretch;
}

.el-header {
    border-bottom: 1px solid #eaecf1;
    width: 60vw;
    align-items: center;
    padding: 20px;
}

.log {
    flex-direction: column;
    background-color: #f5f5f5;
    flex: 0 0 55vh;
    user-select: none;
}

.send-area {
    flex: 0 1 20vh;
}

.user {
    text-align: right;
    width: auto;
    height: auto;
    flex-direction: row-reverse;
    display: flex;
    align-items: center;
}

.bot {
    height: auto;
    width: auto;
    flex-direction: row;
    display: flex;
    align-items: center;
}

.bot span.chat-content {
    display: inline-block;
    background: white;
    color: black;
    padding: 5px 10px;
    margin-left: 10px;
    border-radius: 8px;
    user-select: text;
    margin-right: 100px;
}

.user span.chat-content {
    display: inline-block;
    background: #409EFF;
    color: #fff;
    padding: 5px 10px;
    border-radius: 8px;
    user-select: text;
    margin-right: 10px;
    margin-left: 100px;
}

.el-textarea {
    display: flex;
    height: 15vh;

}

.el-button {
    margin-top: 3px;
    float: right;
    margin-right: 3px;
}

.textarea >>> .el-textarea__inner {
    font-family: "system-ui", serif !important;
    font-size: 16px !important;
    color: black;
}

.el-button {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 35px;
    margin-top: 5px;
}

.el-avatar {
    cursor: default;
    display: flex;
    flex: 0 0 auto;
    align-self: flex-start;
}

.content {
    display: flex;
    align-items: stretch;
    flex-direction: column;
    justify-content: center;
    margin: 10px;
}

.chat-content {
    display: flex;
    flex: 0 1 auto;
}

.prompts {
    display: flex;
    flex-direction: row;
    margin-left: 45px;
    margin-bottom: 10px;
    flex-wrap: wrap;
}

.prompt {
    margin: 10px 10px 0 0;
}
</style>
