import Vue from 'vue';
import Router from 'vue-router';
import QuestionList from "../pages/QuestionList/QuestionList";
import QuestionDetail from "../pages/QuestionDetail/QuestionDetail";
import PersonalCenter from "../pages/PersonalCenter/PersonalCenter";
import Chat from "../pages/Chat/Chat";
import RaiseQuestion from "../pages/RaiseQuestion/RaiseQuestion";
import SearchResult from "../pages/SearchResult/SearchResult";
import CodeAnalysis from "../pages/CodeAnalysis/CodeAnalysis";
import Welcome from "../pages/Welcome/Welcome";

Vue.use(Router)

export default new Router({
    scrollBehavior (to, from) {
        if (from.name === 'questionList') {
            from.meta.savedPosition = document.getElementById('scroll-body').scrollTop;
        }
        else if (from.name === 'chat') {
            from.meta.savedPosition = document.getElementById('words').scrollTop;
        }
        if (to.name === 'questionList' && to.meta.savedPosition) {
            document.getElementById('scroll-body').scrollTop = to.meta.savedPosition;
        }
        else if (to.name === 'chat' && to.meta.savedPosition) {
            document.getElementById('words').scrollTop = to.meta.savedPosition;
        }
    },
    routes: [
        {
            path: '/',
            redirect: 'chat',
        },
        {
            path: '/questionList',
            component: QuestionList,
            meta: {keepAlive: true},
            name: 'questionList'
        },
        {
            path: '/questionDetail',
            component: QuestionDetail,
            meta: {keepAlive: false},
            name: 'questionDetail'
        },
        {
            path: '/raiseQuestion',
            component: RaiseQuestion,
            meta: {keepAlive: true},
            name: 'raiseQuestion'
        },
        {
            path: '/personalCenter',
            component: PersonalCenter
        },
        {
            path: '/chat',
            component: Chat,
            meta: {keepAlive: true},
            name: 'chat'
        },
        {
            path: '/searchResult',
            component: SearchResult,
            name: 'searchResult'
        },
        {
            path: '/codeAnalysis',
            component: CodeAnalysis,
            meta: {show: false},
            name: 'codeAnalysis'
        },
        {
            path: '/personalCenter',
            component: PersonalCenter,
            name: 'personalCenter'
        },
        {
            path: '/welcome',
            component: Welcome,
            name: 'welcome',
            meta: {welcome: true}
        }
    ]
})
