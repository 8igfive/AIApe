import Vue from 'vue'
import Vuex from 'vuex';

Vue.use(Vuex);

export default new Vuex.Store({
    state: {
        show: {
            login: false,
            questionList: false
        },
        questionID: 0,
        username: '',
        uid: 0,
        token: '',
        avatarIndex: 0,
        auth: 0,
        timeout: 0,
        tagList: {},
        lastTokenTime: new Date(),
        logs: [{id: 2, content: '你好，我是AIApe！请先登录！', prompts:[], promptValid: false}],
        mobileStatus: 'chat',
        searchResult: [],
        routerIndex: 0,
        codeAnalysis: {
            code: '',
            issues: []
        },
        questionList: {
            tagState: {},
            selectedTag: [],
            tagSelectValid: true
        },
        template: {
            title: '',
            tags: [],
            remarks: ''
        },
        isMobile: false,
    },
    mutations: {
        setUsername(state, value) {
            state.username = value;
        },
        setQuestionID(state, id) {
            state.questionID = id;
        },
        refreshToken(state, payload) {
            state.token = payload.token;
            state.lastTokenTime = payload.time;
            state.timeout = payload.timeout;
        },
        setAuth(state, auth) {
            state.auth = auth;
        },
        setTagList(state, tagList) {
            state.tagList = tagList;
        },
        addAImessage(state, payload) {
            payload['id'] = 2;
            state.logs.push(payload);
        },
        addUserMessage(state, message) {
            state.logs.push({
                id: 1,
                content: message,
                prompts: [],
                promptValid: false
            })
        },
        setUid(state, uid) {
            state.uid = uid
        }
    }
})
