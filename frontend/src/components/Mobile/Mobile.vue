<template>
    <div>

        <div class="main">
            <MobileLogin v-if="this.$store.state.mobileStatus === 'login'"/>
            <MobileChat v-if="this.$store.state.mobileStatus === 'chat'"/>
            <MobileRegister v-if="this.$store.state.mobileStatus === 'register'"/>
            <MobileQuestionList v-show="this.$store.state.mobileStatus === 'questionList'"/>
            <MobileRaiseQuestion v-if="this.$store.state.mobileStatus === 'raiseQuestion'"/>
            <MobileQuestionDetail v-if="this.$store.state.mobileStatus === 'questionDetail'"/>
            <MobileCodeAnalysis v-if="this.$store.state.mobileStatus === 'codeAnalysis'"/>
            <MobilePersonalCenter v-if="this.$store.state.mobileStatus === 'personalCenter'"/>
        </div>

        <div class="footer">
            <MobileTopBar v-if="this.$store.state.mobileStatus !== 'login'
            && this.$store.state.mobileStatus !== 'register'"/>
        </div>
    </div>
</template>

<script>
import MobileLogin from "./MobileLogin";
import MobileChat from "./MobileChat";
import MobileRegister from "./MobileRegister";
import MobileTopBar from "./MobileTopBar";
import MobileQuestionList from "./MobileQuestionList";
import MobileRaiseQuestion from "./MobileRaiseQuestion";
import MobileQuestionDetail from "./MobileQuestionDetail";
import MobileCodeAnalysis from "./MobileCodeAnalysis";
import MobilePersonalCenter from "./MobilePersonalCenter";

export default {
    name: "Mobile",
    data() {
        return {
            status: 'login',
        }
    },
    components: {
        MobileLogin,
        MobileChat,
        MobileRegister,
        MobileTopBar,
        MobileQuestionList,
        MobileRaiseQuestion,
        MobileQuestionDetail,
        MobileCodeAnalysis,
        MobilePersonalCenter,
    },
    beforeCreate() {
    }
}
</script>

<style scoped>
/*.main {*/
/*    height: calc(100vh - 200px);*/
/*}*/

.footer {
    /*margin-top: 50px;*/
    width: 100vw;
    position: fixed;
    bottom: 0;
    z-index: 2;
}

</style>
