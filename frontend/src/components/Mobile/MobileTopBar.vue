<template>
    <el-header height="20%">

        <el-avatar src="https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png"
                   size="large" @click.native="login"></el-avatar>
        <i class="el-icon-chat-dot-round"
           :class="(this.$store.state.mobileStatus === 'chat')? 'selected' : 'unselected'"
           @click="showChat"></i>

        <i class="el-icon-edit" :class="(this.$store.state.mobileStatus === 'raiseQuestion')? 'selected' : 'unselected'"
           @click="showRaiseQuestion"></i>

        <i class="el-icon-notebook-2" :class="(this.$store.state.mobileStatus === 'questionList'
        || this.$store.state.mobileStatus === 'questionDetail')? 'selected' : 'unselected'"
           @click="showQuestionList"></i>

        <i class="el-icon-cpu" :class="(this.$store.state.mobileStatus === 'codeAnalysis')? 'selected' : 'unselected'"
           @click="showCodeAnalysis"></i>

        <!--        <i class="el-icon-paperclip"-->
        <!--           :class="(this.$store.state.mobileStatus === 'questionDetail')? 'selected' : 'unselected'"-->
        <!--           @click="showQuestionDetail"></i>-->
    </el-header>
</template>

<script>
export default {
    name: "MobileBottomBar",
    data() {
        return {
            index: 0,
        }
    },
    methods: {
        showChat() {
            // alert('herre');
            this.$store.state.mobileStatus = 'chat';
            this.index = 0;
        },
        showRaiseQuestion() {
            this.$store.state.mobileStatus = 'raiseQuestion';
            this.index = 1
        },
        showQuestionList() {
            this.$store.state.mobileStatus = 'questionList';
            this.index = 2;
        },
        showQuestionDetail() {
            this.$store.state.mobileStatus = 'questionDetail';
            this.index = 3;
        },
        showCodeAnalysis() {
            this.$store.state.mobileStatus = 'codeAnalysis';
        },
        login() {
            if(this.$store.state.username === '') {
                this.$store.state.mobileStatus = 'login';
            } else {
                this.$store.state.mobileStatus = 'personalCenter';
            }
        }
    }
}
</script>

<style scoped>

.el-header {
    background: rgba(128, 128, 128, 1);
    width: 100%;
    flex-direction: row;
    align-items: center;
}

.el-avatar {
    margin-left: 2vw;
    margin-top: 2%;
    margin-bottom: 1%;
    cursor: pointer;
}

i {
    margin-left: 12vw;
    margin-right: 0;
    font-size: 8vw;
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

</style>
