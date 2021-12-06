<template>
    <el-aside width="30vw">
        <el-main class="search-question">
            <el-input placeholder="搜索你的问题" v-model="question">
                <i slot="suffix" class="el-input__icon el-icon-search" @click="search" style="cursor: pointer"></i>
            </el-input>
        </el-main>
        <el-main>

        </el-main>
        <el-main>

        </el-main>
        <el-main>
        </el-main>
        <el-main class="info">
            <span>AIApe</span>
            <span>京ICP备 2021007509号-1</span>
            <span>
                联系我们 @2021软件工程DQSJ
            </span>
        </el-main>
    </el-aside>
</template>

<script>
export default {
    data() {
        return {
            question: ''
        }
    },
    methods: {
        search() {
            this.$search(this.question);
        }
    }
}
</script>

<style scoped>
.el-aside {
    flex-direction: column;
    height: 95vh;
    overflow: visible;
}

.el-main {
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    margin: 10px 10px 0;
    background-color: white;
    border-radius: 2px;
}

.search-question {
    flex: 0 0 auto;
    box-shadow: 0 0 0 0;
    border-radius: 4px;
    margin-top: 0;
}

.info {
    box-shadow: 0 0 0 0;
    background-color: rgba(0, 0, 0, 0);
    flex-direction: column;
    align-items: center;
    flex-grow: 0;
}
</style>
