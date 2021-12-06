<template>
    <div class="whole">
        <el-form ref="form" :model="form" label-width="70px">
            <el-form-item label="昵称">
                <el-input v-model="form.name" prefix-icon="el-icon-user" placeholder="请输入昵称"></el-input>
            </el-form-item>
            <el-form-item label="邮箱">
                <el-input v-model="form.email" prefix-icon="el-icon-message" placeholder="请输入邮箱"></el-input>
            </el-form-item>
            <el-form-item label="密码">
                <el-input v-model="form.password" prefix-icon="el-icon-lock" placeholder="请输入密码"
                          show-password></el-input>
            </el-form-item>
            <el-form-item label="确认密码">
                <el-input v-model="form.rePassword" type="password" prefix-icon="el-icon-unlock" placeholder="请确认密码"
                @keyup.native="checkPassword">
                    <i slot="suffix" class="el-input__icon el-icon-error" v-show="rePasswordError" @click="form.rePassword=''"></i>
                </el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="onSubmit" :disabled="rePasswordError">注册</el-button>
            </el-form-item>
        </el-form>
    </div>
</template>

<script>
export default {
    data() {
        return {
            form: {
                name: '',
                email: '',
                password: '',
                rePassword: ''
            },
            rePasswordError: false,
        }
    },
    methods: {
        onSubmit() {
            let _this = this;
            _this.$axios.post(_this.BASE_URL + "/api/user/signup", {
                name: _this.$data.form.name,
                email: _this.$data.form.email,
                password: _this.$data.form.password
            })
            .then(function (response) {
                console.log(response);
                let status = response.data.status;
                if (status === 'success') {
                    _this.$message({
                        message: '注册成功!即将跳转至登录页面...',
                        type: 'success'
                    });
                    setTimeout(() => {
                        _this.$router.replace('/login');
                    }, 2000);
                }
                else if (status === 'nameInvalid') {
                    _this.$message({
                        message: '昵称格式不合法',
                        type: 'warning'
                    })
                }
                else if (status === 'nameExisted') {
                    _this.$message({
                        message: '昵称已存在',
                        type: 'warning'
                    })
                }
                else if (status === 'emailInvalid') {
                    _this.$message({
                        message: '邮箱格式错误',
                        type: 'warning'
                    })
                }
                else if (status === 'emailExisted') {
                    _this.$message({
                        message: '邮箱已被注册',
                        type: 'warning'
                    })
                }
                else if (status === 'passwordInvalid') {
                    _this.$message({
                        message: '密码长度应大于8位！',
                        type: 'warning'
                    })
                }
            })
        },
        checkPassword() {
            this.$data.rePasswordError = this.$data.form.password !== this.$data.form.rePassword;
        }
    }
}
</script>

<style scoped>

.whole {
    height: 100%;
    width: 100%;
    padding: 0;
    margin: 0;
}

.el-form {
    position: absolute;
    left: 32.5%;
    top: 30%;
}

.el-button {
    position: absolute;
    left: 32.5%;
}

.el-alert {
    position: absolute;
    top: 0;
    left: 27.5%;
    width: 50%;
    height: 40px;
}
</style>
