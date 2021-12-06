<template>
    <div>
        <h1 class="title">欢迎来到AIAPE</h1>
        <el-image
            class="image"
            :src="require('../../assets/bot.png')"
            :fit="fit"></el-image>
        <div class="form">
            <el-form label-position="left" label-width="20%" :model="loginForm">
                <el-form-item label="邮箱">
                    <el-input prefix-icon="el-icon-message" v-model="loginForm.email"></el-input>
                </el-form-item>
                <el-form-item label="密码">
                    <el-input type="password" show-password prefix-icon="el-icon-lock"
                              v-model="loginForm.password"></el-input>
                </el-form-item>
            </el-form>
        </div>
        <div align="center">
            <el-button type="primary" v-on:click="submit"
                       :disabled="loginForm.password === '' || loginForm.email === ''">登录
            </el-button>
        </div>
        <div align="center" class=gtr>
            <el-link :underline="true" target="_blank" v-on:click="goToRegister">没有账号？点我注册</el-link>
        </div>
    </div>
</template>

<script>

export default {
    name: "MobileLogin.vue",
    data() {
        return {
            loginForm: {
                email: '',
                password: '',
            }
        }
    },
    methods: {
        submit() {
            // alert(this.loginForm.email);
            // alert(this.loginForm.password);
            // this.$store.state.mobileStatus = 'chat';
            let _this = this;
            this.$axios.post(this.BASE_URL + "/api/user/login", {
                email: _this.$data.loginForm.email,
                password: _this.$data.loginForm.password
            })
                .then(function (response) {
                    if (response.data.status === "fail") {
                        _this.$message({
                            message: '账号或密码错误!',
                            type: 'error'
                        });
                    } else {
                        _this.$message({
                            message: '登录成功!',
                            type: 'success'
                        });
                        _this.$store.commit('refreshToken', {
                            token: response.data.token,
                            time: new Date(),
                            timeout: response.data.timeout
                        });
                        _this.$store.commit('setAuth', response.data.auth);
                        _this.$store.state.mobileStatus = "chat";
                        _this.$axios.get(_this.BASE_URL + '/api/user/internal_info', {
                            headers: {
                                Authorization: 'Bearer ' + response.data.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(function (response) {
                                _this.$store.commit('setUsername', response.data.name);
                                _this.$store.commit('setUid', response.data.uid);
                            })
                            .catch(function (error) {
                                console.log(error);
                            });
                        // alert(_this.$store.state.token);
                    }
                })
        },
        goToRegister() {
            this.$store.state.mobileStatus = 'register';
        }
    }

}
</script>

<style scoped>
.title {
    text-align: center;
}

.image {
    width: 30%;
    height: 30%;
    margin-top: 0;
    margin-bottom: 5%;
    margin-right: 35%;
    margin-left: 35%;
}

.form {
    width: 70%;
    margin-left: 10%;
    margin-right: 20%;
}

.gtr {
    margin-top: 10%;
}

</style>
