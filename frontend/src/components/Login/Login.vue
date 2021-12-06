<template>
    <div class="shell" style="z-index: 9999">
        <el-container>
            <el-header>
                <i class="el-icon-circle-close" style="cursor: pointer" @click="close"></i>
                <span v-show="false">登录</span>
                <span v-show="false">注册</span>
                <span style="font-size: 30px">AIApe</span>
            </el-header>
            <el-main class="login" v-show="show.login">
                <el-form ref="form" :rules="loginRule" :model="loginForm" label-width="0">
                    <el-form-item prop="email">
                        <el-input v-model="loginForm.email" prefix-icon="el-icon-message"
                                  placeholder="请输入邮箱"></el-input>
                    </el-form-item>
                    <el-form-item prop="password">
                        <el-input v-model="loginForm.password" prefix-icon="el-icon-lock" @keyup.native.enter="login"
                                  placeholder="请输入密码" prop="password" show-password></el-input>
                    </el-form-item>
                    <el-button type="primary" @click="login">登录</el-button>
                    <el-link type="info" @click="switchPage">还没有账号?点击注册</el-link>
                </el-form>
            </el-main>
            <el-main class="register" v-show="show.register">
                <el-form ref="registerForm" :model="registerForm" :rules="rules" label-width="0">
                    <el-form-item prop="name">
                        <el-input v-model="registerForm.name" prefix-icon="el-icon-user" placeholder="请输入昵称"></el-input>
                    </el-form-item>
                    <el-form-item prop="email">
                        <el-input v-model="registerForm.email" prefix-icon="el-icon-message"
                                  placeholder="请输入邮箱"></el-input>
                    </el-form-item>
                    <el-form-item prop="password">
                        <el-input v-model="registerForm.password" prefix-icon="el-icon-lock" placeholder="请输入密码"
                                  show-password></el-input>
                    </el-form-item>
                    <el-form-item prop="rePassword">
                        <el-input v-model="registerForm.rePassword" type="password" prefix-icon="el-icon-unlock"
                                  placeholder="请确认密码">
                            <i slot="suffix" class="el-input__icon el-icon-error" v-show="rePasswordError"
                               @click="registerForm.rePassword=''"></i>
                        </el-input>
                    </el-form-item>
                    <el-button type="primary" @click="register">注册</el-button>
                    <el-link type="info" @click="switchPage">已有账号?点击登录</el-link>
                </el-form>
            </el-main>
        </el-container>
    </div>
</template>

<script>
export default {
    data() {
        let validEmail = (rule, value, callback) => {
            if (value === '') {
                callback(new Error('请输入邮箱'));
            } else if (!this.isEmail(value)) {
                callback(new Error('邮箱格式不正确'));
            } else {
                callback();
            }
        };
        let validatePass2 = (rule, value, callback) => {
            if (value === '') {
                callback(new Error('请再次输入密码'));
            } else if (value !== this.registerForm.password) {
                callback(new Error('两次输入密码不一致!'));
            } else {
                callback();
            }
        };
        return {
            loginForm: {
                email: '',
                password: ''
            },
            registerForm: {
                name: '',
                email: '',
                password: '',
                rePassword: ''
            },
            show: {
                login: true,
                register: false
            },
            rePasswordError: false,
            loginRule: {
                email: [
                    {validator: validEmail, trigger: 'blur'}
                ],
                password: [
                    {required: true, message: '请输入密码', trigger: 'blur'},
                ],
            },
            rules: {
                name: [
                    {required: true, message: '请输入昵称', trigger: 'blur'}
                ],
                email: [
                    {validator: validEmail, trigger: 'blur'}
                ],
                password: [
                    {required: true, message: '请输入密码', trigger: 'blur'},
                    {min: 8, max: 20, message: '密码应为8~20位', trigger: 'blur'}
                ],
                rePassword: [
                    {validator: validatePass2, trigger: 'blur'}
                ]
            }
        }
    },
    methods: {
        login() {
            let v = false;
            this.$refs['form'].validate((valid) => {
                if (!valid) {
                    this.$message({
                        message: '登录失败',
                        type: 'error'
                    })
                }
                v = valid;
            });
            if (!v) {
                return;
            }
            let _this = this;
            this.$axios.post(this.BASE_URL + "/api/user/login", {
                email: _this.loginForm.email,
                password: _this.loginForm.password
            })
                .then(function (response) {
                    if (response.data.status === "fail") {
                        _this.$message({
                            message: '邮箱或密码错误!',
                            type: 'error'
                        });
                    } else {
                        _this.$message({
                            message: '登录成功!',
                            type: 'success'
                        });
                        // _this.GLOBAL.token = response.data.token;
                        _this.$store.commit('refreshToken', {
                            token: response.data.token,
                            time: new Date(),
                            timeout: response.data.timeout
                        });
                        _this.$store.commit('setAuth', response.data.auth);
                        _this.$store.state.show.login = false;
                        _this.$axios.get(_this.BASE_URL + '/api/user/internal_info', {
                            headers: {
                                Authorization: 'Bearer ' + response.data.token,
                                type: 'application/json;charset=utf-8'
                            }
                        })
                            .then(function (response) {
                                _this.$store.commit('setUsername', response.data.name);
                                _this.$store.commit('setUid', response.data.uid);
                                _this.$store.state.avatarIndex = response.data.profilePhoto;
                            })
                            .catch(function (error) {
                                console.log(error);
                            });
                    }
                })
        },
        register() {
            let v = false;
            this.$refs['registerForm'].validate((valid) => {
                if (!valid) {
                    this.$message({
                        message: '请按照提示修改注册信息！',
                        type: 'error'
                    })
                }
                v = valid;
            });
            if (!v) {
                return;
            }
            let _this = this;
            _this.$axios.post(_this.BASE_URL + "/api/user/signup", {
                name: _this.registerForm.name,
                email: _this.registerForm.email,
                password: _this.registerForm.password
            })
                .then(function (response) {
                    let status = response.data.status;
                    if (status === 'success') {
                        _this.$message({
                            message: '注册成功!即将跳转至登录页面...',
                            type: 'success'
                        });
                        setTimeout(() => {
                            _this.switchPage();
                        }, 2000);
                    } else if (status === 'nameInvalid') {
                        _this.$message({
                            message: '昵称格式不合法',
                            type: 'warning'
                        })
                    } else if (status === 'nameExisted') {
                        _this.$message({
                            message: '昵称已存在',
                            type: 'warning'
                        })
                    } else if (status === 'emailInvalid') {
                        _this.$message({
                            message: '邮箱格式错误',
                            type: 'warning'
                        })
                    } else if (status === 'emailExisted') {
                        _this.$message({
                            message: '邮箱已被注册',
                            type: 'warning'
                        })
                    } else if (status === 'passwordInvalid') {
                        _this.$message({
                            message: '密码长度应大于8位！',
                            type: 'warning'
                        })
                    }
                })

        },
        switchPage() {
            this.show.login = !this.show.login;
            this.show.register = !this.show.register;
        },
        gotoAdministration() {
            this.$router.replace('/administration');
        },
        close() {
            this.$store.state.show.login = false;
        },
        isEmail(s) {
            return /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((.[a-zA-Z0-9_-]{2,3}){1,2})$/.test(s);
        }
    },
    // directives: {
    //     drag(el) {
    //         el.onmousedown = function (e) {
    //             var disx = e.pageX - el.offsetLeft
    //             var disy = e.pageY - el.offsetTop
    //             document.onmousemove = function (e) {
    //                 el.style.left = e.pageX - disx + 'px'
    //                 el.style.top = e.pageY - disy + 'px'
    //             }
    //             document.onmouseup = function () {
    //                 document.onmousemove = document.onmouseup = null
    //             }
    //         }
    //     }
    // }
}
</script>

<style scoped>

.shell {
    position: absolute;
    border: 1px solid lightgrey;
    left: 35vw;
    top: 25vh;
    width: 30vw;
    height: 50vh;
    background-color: white;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
    border-radius: 5px;
}

.el-main {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.el-main > * {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.el-input {
    width: 220px;
}

.el-button {
    margin-bottom: 15px;
}

.el-header {
    /*border-bottom: 1px solid #eaecf1;*/
    align-items: center;
    height: 10vh;
    width: 30vw;
    flex-direction: column;
}

.el-icon-circle-close {
    align-self: flex-end;
    color: lightgrey;
    margin: 2px;
}

.el-main.login {
    margin-top: 10vh;
}

.el-main.register {
    margin-top: 3vh;
}

</style>
