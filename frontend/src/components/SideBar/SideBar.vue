<template>
    <el-aside width="5vw" v-show="this.$store.state.routerIndex < 7">
        <div style="display: flex; flex-direction: column; align-items: center">
            <el-popover placement="right-end" trigger="hover" :title="this.$store.state.username" content="欢迎使用AIApe"
                        v-if="this.$store.state.username !== ''">
                <el-button type="text" @click="logout" style="float: right">退出登录</el-button>
                <el-avatar :src="avatarSrc"
                           size="large" @click.native="login" slot="reference"></el-avatar>
            </el-popover>
            <el-popover placement="right-end" trigger="hover" title="点击登录" content="登录后查看更多精彩内容！" v-else>
                <el-avatar :src="avatarSrc"
                           size="large" @click.native="login" slot="reference"></el-avatar>
            </el-popover>
            <i class="el-icon-chat-dot-round" :class="(index === 0)? 'selected' : 'unselected'"
               @click="changePage(0);"></i>
            <i class="el-icon-notebook-2" :class="(index === 2)? 'selected' : 'unselected'" @click="changePage(2);"></i>
            <i class="el-icon-edit" :class="(index === 1)? 'selected' : 'unselected'" @click="changePage(1);"
               v-show="this.$store.state.username !== ''"></i>
            <i class="el-icon-paperclip" :class="(index === 3)? 'selected' : 'unselected'" @click="changePage(3);"
               v-show="questionDetailExist"></i>
            <i class="el-icon-search" :class="(index === 4)? 'selected' : 'unselected'" @click="changePage(4);"
               v-show="searchResultExist"></i>
            <i class="el-icon-cpu" :class="(index === 5)? 'selected' : 'unselected'" @click="changePage(5);"
               v-show="this.$store.state.username !== ''"></i>
        </div>
        <i class="el-icon-question" @click="openWelcome"></i>
    </el-aside>
</template>

<script>
export default {
    data() {
        return {}
    },
    computed: {
        index() {
            return this.$store.state.routerIndex;
        },
        questionDetailExist() {
            return this.$store.state.questionID > 0;
        },
        searchResultExist() {
            return this.$store.state.searchResult.length > 0;
        },
        avatarSrc() {
            return 'http://81.70.211.128/aiape/icon-avatar' + this.$store.state.avatarIndex + '.png'
        }
    },
    methods: {
        login() {
            if (this.$store.state.username === '') {
                this.$store.state.show.login = true;
            } else {
                this.$changePage(6);
            }
        },
        changePage(index) {
            this.$changePage(index);
        },
        logout() {
            this.$store.state.username = '';
            this.$store.state.uid = 0;
            this.$store.state.token = '';
            this.$store.state.avatarIndex = 0;
            this.$changePage(0);
        },
        openWelcome() {
            let path = this.$router.resolve({
                path: '/welcome'
            })
            window.open(path.href, '_blank');
        }
    },
    watch: {}
}
</script>

<style scoped>

.el-aside {
    background: rgba(0, 0, 0, 0.4);
    height: 100vh;
    flex-direction: column;
    align-items: center;
    justify-content: space-between;
}

.el-avatar {
    margin-top: 20px;
    cursor: pointer;
}

i {
    margin-top: 40px;
    font-size: 30px;
    cursor: pointer;
}

i:hover {
    color: lightgrey;
}

.selected {
    color: lightgrey;
}

.unselected {
    color: black;
}

.el-icon-question {
    margin-bottom: 30px;
}
</style>
