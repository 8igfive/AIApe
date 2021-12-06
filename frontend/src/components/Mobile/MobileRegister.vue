<template>
    <div>
        <h1 class="title">AIAPE-注册</h1>
        <div class="form">
            <el-form label-position="left" label-width="25%" :model="registerForm">
                <el-form-item label="昵称">
                    <el-input prefix-icon="el-icon-user" v-model="registerForm.nickName"></el-input>
                </el-form-item>
                <el-form-item label="邮箱">
                    <el-input prefix-icon="el-icon-message" v-model="registerForm.email"></el-input>
                </el-form-item>
                <el-form-item label="密码">
                    <el-input type="password" prefix-icon="el-icon-lock" show-password
                              v-model="registerForm.password" @keyup.native="checkPswLength"></el-input>
                    <p v-if="pswLengthError" class="warning">密码长度不足8位</p>
                </el-form-item>
                <el-form-item label="确认密码">
                    <el-input type="password" prefix-icon="el-icon-unlock" show-password
                              v-model="registerForm.rePassword" @keyup.native="checkPassword"></el-input>
                    <p v-if="rePasswordError" class="warning">两次密码不一致</p>
                </el-form-item>
            </el-form>
        </div>
        <div align="center">
            <el-button type="primary" v-on:click="register"
                       :disabled="pswLengthError || rePasswordError || registerForm.nickName === ''
                       || registerForm.password === '' || registerForm.email === ''
                       || registerForm.rePassword === ''">注册
            </el-button>
        </div>

        <div align="center" class=gtl>
            <el-link :underline="true" target="_blank" v-on:click="goToLogin">返回登陆</el-link>
        </div>
    </div>
</template>

<script>
export default {
    name: "MobileRegister",
    data() {
        return {
            registerForm: {
                nickName: '',
                email: '',
                password: '',
                rePassword: '',
            },
            rePasswordError: false,
            pswLengthError: false,
        }
    },
    methods: {
        register() {
            // alert('here!')
            let _this = this;
            _this.$axios.post(_this.BASE_URL + "/api/user/signup", {
                name: _this.$data.registerForm.nickName,
                email: _this.$data.registerForm.email,
                password: _this.$data.registerForm.password
            })
                .then(function (response) {
                    let status = response.data.status;
                    // alert(status);
                    if (status === 'success') {
                        _this.$message({
                            message: '注册成功!即将跳转至登录页面...',
                            type: 'success'
                        });
                        setTimeout(() => {
                            _this.$store.state.mobileStatus = "login";
                        }, 2000);
                    } else if (status === 'nameInvalid') {
                        _this.$message({
                            message: "昵称格式不合法",
                            type: 'error'
                        });
                    } else if (status === 'nameExisted') {
                        _this.$message({
                            message: "昵称已存在",
                            type: 'error'
                        });
                    } else if (status === 'emailExisted') {
                        _this.$message({
                            message: "邮箱已注册",
                            type: 'error'
                        });
                    } else if (status === 'emailInvalid') {
                        _this.$message({
                            message: "邮箱格式错误",
                            type: 'error'
                        });
                    }
                })
                .catch(function (error) {
                    console.log(error);
                })
        },
        goToLogin() {
            this.$store.state.mobileStatus = 'login';
        },
        checkPassword() {
            this.$data.rePasswordError = this.$data.registerForm.password !== this.$data.registerForm.rePassword;
        },
        checkPswLength() {
            this.$data.pswLengthError = this.$data.registerForm.password.length < 8;
            this.$data.rePasswordError = this.$data.registerForm.password !== this.$data.registerForm.rePassword;
        }
    }
}
</script>

<style scoped>

.title {
    text-align: center;
}

.form {
    width: 70%;
    margin-left: 10%;
    margin-right: 20%;
}

.gtl {
    margin-top: 10%;
}

.warning {
    margin-top: -5px;
    margin-bottom: -20px;
    color: red;
}

</style>
