// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import 'github-markdown-css/github-markdown.css';
import 'vue2-animate/dist/vue2-animate.min.css';
import mavonEditor from 'mavon-editor';
import 'mavon-editor/dist/css/index.css';
import Meta from 'vue-meta';
import ElementUI from 'element-ui';
Vue.use(ElementUI);
import 'element-ui/lib/theme-chalk/index.css';
import Vue from 'vue'
import App from './App'
import store from "./vuex/store";
import axios from 'axios';
import router from './router';
import marked from "marked";
import VueParticles from 'vue-particles';
import './common/font/font.css';

Vue.use(VueParticles);
Vue.prototype.$axios = axios;
// const BASE_URL = 'https://aiape.snowphoenix.design';
const BASE_URL = 'http://test.snowphoenix.design';
Vue.prototype.BASE_URL = BASE_URL;

Vue.use(mavonEditor);
Vue.use(Meta);
Vue.prototype.$store = store;

new Vue({
    el: '#app',
    render: h => h(App),
    router
})

axios.interceptors.response.use(response => {
    // 几种不需要刷新token的情况
    if (store.state.token === '' || response.data.message === 'token fresh' ||
        new Date().getTime() - store.state.lastTokenTime < store.state.timeout / 2) {
        return response;
    }

    let current = new Date();
    let existTime = (current.getTime() - store.state.lastTokenTime.getTime()) / 1000;

    if (existTime > store.state.timeout) {
        alert('登录超时!');
        store.state.username = '';
        store.state.uid = 0;
        store.state.token = '';
        store.state.routerIndex = 0;
        if (store.state.isMobile) {
            window.location.reload();
        } else {
            router.replace('/chat');
        }
        // router.replace('/login');
        // this.$store.state.mobileStatus = 'login';
        // this.$store.state.username = '';
    } else {
        axios.post(BASE_URL + '/api/user/fresh', {
            token: store.state.token
        })
            .then(function (ret) {
                if (ret.data.state === 'success') {
                    store.commit('refreshToken', {
                        token: ret.data.token,
                        time: new Date(),
                        timeout: ret.data.timeout
                    })
                }
            })
            .catch(function (error) {
                console.log(error);
            })
        return response;
    }
})

Vue.prototype.$search = function (key) {
    let _this = this;
    this.$store.state.searchResult = [];
    this.$axios.post(this.BASE_URL + '/api/questions/search', {
        content: key
    })
        .then(async function (response) {
            console.log(response);
            let questions = response.data;
            for (let qid of questions) {
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
                        _this.$store.state.searchResult.push(question);
                    });
            }
        })
        .catch(function (error) {
            console.log(error);
        })
    this.$router.replace('/searchResult');
}

Vue.prototype.$changePage = function (index) {
    if (index === this.$store.state.routerIndex) {
        return;
    }
    this.$store.state.routerIndex = index;
    switch (index) {
        case 0: {
            this.$router.replace('/chat');
            break;
        }
        case 1: {
            this.$router.replace('/raiseQuestion');
            break;
        }
        case 2: {
            this.$router.replace('/questionList');
            break;
        }
        case 3: {
            this.$router.replace('/questionDetail');
            break;
        }
        case 4: {
            this.$router.replace('/searchResult');
            break;
        }
        case 5: {
            this.$router.replace('/codeAnalysis');
            break;
        }
        case 6: {
            this.$router.replace('/personalCenter');
            break;
        }
    }
}


