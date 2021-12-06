<template>
    <div>
        <el-header height="5vh">
            AIApe
        </el-header>
        <div style="position: fixed; top: 5vh">
            <el-main class="log" id="scroll">
                <div style="width: 100vw; margin-top: 1vh">
                    <div v-for="msg in this.$store.state.logs" class="content" :class="msg.id === 1? 'user':'bot'">
                        <el-avatar
                            :src="msg.id === 1? 'https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png': bot_avatar"
                            size="medium"></el-avatar>
                        <span class="chat-content" v-html="msg.content">
                        {{ msg.content }}
                    </span>
                        <br/>
                        <el-button class="prompt" v-for="prompt in msg.prompts"
                                   :key="prompt" v-show="msg.promptValid"
                                   @click="choosePrompt(prompt, msg)">
                            {{ prompt }}
                        </el-button>
                    </div>
                </div>
            </el-main>
            <el-main class="send-area">
                <div style="width: 100vw; height: 20vh">
                    <el-input class="textarea" type="textarea" resize="none" v-model="message"></el-input>
                    <el-button type="primary" v-on:click="send">发送</el-button>
                </div>
            </el-main>
        </div>
    </div>
</template>

<script>
export default {
    name: "MobileChat",
    data() {
        return {
            message: '',
            bot_avatar: require('../../assets/bot.png'),
        }
    },
    computed: {
        username() {
            return this.$store.state.username;
        },
        logs() {
            return this.$store.state.logs;
        },
        prompt() {
            return this.$store.state.prompt;
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
                .then(function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = _this.transform(message);
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
                        if (message.indexOf('el-link') !== -1) {
                            location.reload();
                        }
                    }
                    // let ss = document.getElementById("scroll");
                    // ss.scrollTop = ss.scrollHeight;
                })
                .catch(function (error) {
                })
            this.$data.message = '';

            setTimeout(function () {
                let ss = document.getElementById("scroll");
                ss.scrollTop = ss.scrollHeight;
            },50);

        },
        getTagList() {
            let _this = this;
            _this.$axios.get(_this.BASE_URL + '/api/questions/taglist')
                .then(function (response) {
                    let tagList = response.data;
                    _this.$store.commit('setTagList', tagList);
                })
        },
        transform(msg) {
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
                    this.$store.commit('setQuestionID', id);
                    msg = msg.substring(0, left) +
                        '<el-link type="text" @click="gotoQuestionDetail()">' +
                        'Question' + id + '</el-link>' + msg.substring(right + 1);
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
                .then(function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = _this.transform(message);
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
        gotoQuestionDetail() {
            this.$store.state.mobileStatus = 'questionState';
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
                .then(function (response) {
                    let i = 0;
                    for (let message of response.data.messages) {
                        message = _this.transform(message);
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
                this.$refs['words'].scrollTop = this.$refs['words'].scrollHeight;
            })
        }
    },
    mounted() {

    },
    created() {
        this.$nextTick(() => {
            this.$refs['words'].scrollTop = this.$refs['words'].scrollHeight;
        })
    }
}
</script>

<style scoped>

.el-header {
    position: fixed;
    border-bottom: 1px solid #eaecf1;
    width: 100vw;
    align-items: center;
    padding: 5%;
}

.log {
    flex-direction: column;
    background-color: #f5f5f5;
    /*flex: 0 0 55vh;*/
    height: 53vh;
    user-select: none;
}

.send-area {
    /*flex: 0 1 25vh;*/
    height: 30vh;
}

.user {
    margin: 2vw;
    text-align: right;
    width: auto;
    height: auto;
}

.bot {
    height: auto;
    margin: 2vw;
    width: auto;
}

.bot span.chat-content {
    display: inline-block;
    background: white;
    color: black;
    padding: 5px 10px;
    margin-left: 2vw;
    border-radius: 8px;
    user-select: text;
    margin-right: 20vw;
}

.user span.chat-content {
    display: inline-block;
    background: #409EFF;
    color: #fff;
    padding: 5px 10px;
    border-radius: 8px;
    user-select: text;
    margin-right: 2vw;
    margin-left: 20vw;
}

.el-textarea {
    display: flex;
    height: 15vh;

}

.el-button {
    /*margin-top: 30px;*/
    float: right;
    margin-right: 1vw;
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
    height: 4vh;
    margin-top: 0.6vh;
}

.el-avatar {
    cursor: default;
    display: flex;
    flex: 0 0 auto;
    align-self: flex-start;
}

.content.user {
    flex-direction: row-reverse;
}

.content.bot {
    flex-direction: row;
}

.content {
    display: flex;
    align-items: center;
}

.chat-content {
    display: flex;
    flex: 0 1 auto;
}

</style>
