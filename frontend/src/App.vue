<template>
    <div v-if="isMobile">
        <Mobile/>
    </div>
    <div v-else>
        <el-container>
            <div class="background" v-if="!$route.meta.welcome">
                <img :src="imgSrc" width="100%" height="100%" alt=""/>
            </div>
            <Login v-show="this.$store.state.show.login"></Login>
            <SideBar v-if="!$route.meta.welcome"></SideBar>
            <transition
                name="zoom"
                enter-active-class="zoomInLeft"
                leave-active-class="zoomOutLeft">
                <keep-alive>
                    <router-view v-if="$route.meta.keepAlive"></router-view>
                </keep-alive>
            </transition>
            <transition
                name="zoom"
                enter-active-class="zoomInLeft"
                leave-active-class="zoomOutLeft">
            <router-view v-if="!$route.meta.keepAlive"></router-view>
            </transition>
        </el-container>
    </div>
</template>

<script src="https://cdn.bootcss.com/showdown/1.3.0/showdown.min.js"></script>
<script>
import Chat from './pages/Chat/Chat.vue'
import SideBar from "./components/SideBar/SideBar";
import Login from "./components/Login/Login";
import QuestionList from "./pages/QuestionList/QuestionList";
import Mobile from "./components/Mobile/Mobile";

window.onload = function () {
    document.addEventListener('touchstart', function (event) {
        if (event.touches.length > 1) {
            event.preventDefault()
        }
    })
    document.addEventListener('gesturestart', function (event) {
        event.preventDefault()
    })
}

export default {
    data() {
        return {
            imgSrc: require('./assets/background1.jpg'),
            system: {
                win: false,
                mac: false,
                xll: false,
            },
            isMobile: false,
        }
    },
    components: {
        Mobile,
        SideBar,
        Chat,
        Login,
        QuestionList
    },
    created() {
        let _this = this;
        var p = navigator.platform;
        this.system.win = p.indexOf("Win") == 0;
        this.system.mac = p.indexOf("Mac") == 0;
        this.system.xll = p.indexOf("Xll") == 0;
        this.isMobile = !(this.system.win || this.system.mac || this.system.xll);
        this.$store.state.isMobile = this.isMobile;
        // this.isMobile = true;
        // this.$store.state.mobileStatus = 'questionList';

        //在页面加载时读取localStorage里的状态信息
        if (sessionStorage.getItem("store")) {
            this.$store.replaceState(Object.assign({}, this.$store.state, JSON.parse(sessionStorage.getItem("store"))))
        }

        //在页面刷新时将vuex里的信息保存到localStorage里
        window.addEventListener("beforeunload", () => {
            sessionStorage.setItem("store", JSON.stringify(this.$store.state))
        })

        this.$store.state.lastTokenTime = new Date();

        this.$axios.get(this.BASE_URL + '/api/questions/tagcategory')
            .then(function (response) {
                _this.$store.state.tagList = response.data;
            })
    }
}
</script>

<style>
@import "./common/font/font.css";

body {
    margin: 0;
    overflow-y: scroll;
    overflow-x: hidden;
    font-family: msyh, Georgia;
}

.background {
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
    position: absolute;
    user-select: none;
}

.el-main, .el-aside, .el-header {
    display: flex;
    padding: 0;
}

.el-container {
    align-items: center;
}

::-webkit-scrollbar {
    width: 0!important;
}

::-webkit-scrollbar-thumb:hover {
    background-color: #eaecf1;
    border-radius: 5px!important;
}

.v-show-content {
    background-color: white !important;
}

.CodeMirror {
    height: 80vh!important;
}
</style>
