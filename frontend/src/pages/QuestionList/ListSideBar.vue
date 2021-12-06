<template>
    <el-aside width="30vw">
        <el-main class="search-question">
            <el-input placeholder="搜索你的问题" v-model="question">
                <i slot="suffix" class="el-input__icon el-icon-search" @click="search" style="cursor: pointer"></i>
            </el-input>
        </el-main>
        <el-main class="category">
            <span>环境</span>
            <div class="tag-selector">
                <el-tag v-for="(tid, tname) in this.$store.state.tagList.Env" :key="tid"
                        :effect="tagState[tid]? 'dark' : 'light'" @click="select(tid)" v-show="showTag"
                        :class="{'unclickable': !tagSelectValid}">
                    {{ tname }}
                </el-tag>
            </div>
        </el-main>
        <el-main class="category">
            <span>语言</span>
            <div class="tag-selector">
                <el-tag v-for="(tid, tname) in this.$store.state.tagList.Lang" :key="tid"
                        :effect="tagState[tid]? 'dark' : 'light'" @click="select(tid)" v-show="showTag"
                        :class="{'unclickable': !tagSelectValid}">
                    {{ tname }}
                </el-tag>
            </div>
        </el-main>
        <el-main class="category">
            <span>其它</span>
            <div class="tag-selector">
                <el-tag v-for="(tid, tname) in this.$store.state.tagList.Other" :key="tid"
                        :effect="tagState[tid]? 'dark' : 'light'" @click="select(tid)" v-show="showTag"
                        :class="{'unclickable': !tagSelectValid}">
                    {{ tname }}
                </el-tag>
            </div>

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
            question: '',
            showTag: true
        }
    },
    methods: {
        search() {
            this.$search(this.question);
        },
        select(tid) {
            if (!this.tagSelectValid) {
                return;
            }
            this.$store.state.questionList.tagState[tid] = !this.$store.state.questionList.tagState[tid];
            this.showTag = false;
            this.showTag = true;
            let selectedTag = [];
            let tagState = this.$store.state.questionList.tagState;
            for (let id in tagState) {
                if (this.tagState[id]) {
                    selectedTag.push(id);
                }
            }
            this.$store.state.questionList.selectedTag = selectedTag;
        }
    },
    computed: {
        tagState() {
            return this.$store.state.questionList.tagState;
        },
        tagSelectValid() {
            return this.$store.state.questionList.tagSelectValid;
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

.category {
    flex-direction: column;
    padding: 20px;
}

.el-tag {
    margin-top: 10px;
    margin-right: 10px;
    cursor: pointer;
}

i {
    color: #409eff;
}

i:hover {
    color: #6dfff3;
}

.unclickable {
    cursor: default;
}
</style>
