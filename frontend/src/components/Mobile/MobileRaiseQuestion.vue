<template>
    <el-container>
        <el-header height="5vh">
            AIApe
        </el-header>
        <el-footer>
            <el-steps direction="horizontal" :active="step" align-center="true">
                <el-step title="问题标题"></el-step>
                <el-step title="选择标签"></el-step>
                <el-step title="问题详情"></el-step>
            </el-steps>
        </el-footer>
        <el-container class="main-body">
            <el-main>
                <div class="edit-title" v-show="step === 1">
                    <el-input v-show="step === 1" v-model="title" placeholder="请输入问题的标题"></el-input>
                    <el-button v-show="step === 1" @click="submitTitle">提交</el-button>
                </div>

                <transition name="fade">
                    <div class="title" v-show="step > 1">
                        <h1>{{ title }}</h1>
                    </div>
                </transition>

                <transition name="slide" enter-active-class="slideInUp" leave-active-class="none">
                    <div class="select-tag" v-show="step === 2">
                        <div style="margin-left: 10vw" v-for="(ls, type) in tags" class="tag-type">
                            <span>{{ type }}</span>
<!--                            <el-tag v-for="tag in ls" :key="tag">{{ tag }}</el-tag>-->
                            <el-tag style="margin-bottom: 1vh" class="selector" v-for="(tid, tname) in ls" :key="tid"
                                    :effect="tagState[tid]? 'dark' : 'light'" @click="select(tid, tname)" v-show="showTag">
                                {{ tname }}
                            </el-tag>
                        </div>
                        <el-button type="primary" @click="submitTags">完成</el-button>
                    </div>
                </transition>


                <transition name="fade">
                    <div class="tag" v-show="step > 2">
                        <el-tag v-for="tag in selectTags" :key="tag">{{ tag }}</el-tag>
                    </div>
                </transition>
                <transition name="slide" enter-active-class="slideInUp">
                    <div class="edit-detail" v-show="step === 3">
                        <mavon-editor :toolbars="toolbars" v-model="detail" ref=md
                                      :subfield="prop.subfield" :defaultOpen="prop.defaultOpen"
                                      :toolbarsFlag="prop.toolbarsFlag" :editable="prop.editable"
                                      :scrollStyle="prop.scrollStyle" :boxShadow="prop.boxShadow"
                                      placeholder="详细描述你的问题...">

                        </mavon-editor>
                        <el-button type="primary" v-on:click="submitQuestion">提交</el-button>
                    </div>
                </transition>
            </el-main>

        </el-container>
<!--        <el-footer>-->
<!--            <el-steps direction="horizontal" :active="step" align-center="true">-->
<!--                <el-step title="问题标题"></el-step>-->
<!--                <el-step title="选择标签"></el-step>-->
<!--                <el-step title="问题详情"></el-step>-->
<!--            </el-steps>-->
<!--        </el-footer>-->
    </el-container>
</template>

<script>
export default {
    name: "MobileRaiseQuestion",
    data() {
        return {
            step: 1,
            title: '',
            detail: '',
            selectedTag: [],
            tags: {
                '环境': {}, '语言': {}, '其它': {}
            },
            tagState: [],
            toolbars: {
                bold: true, // 粗体
                italic: true, // 斜体
                header: true, // 标题
                underline: true, // 下划线
                strikethrough: false, // 中划线
                mark: false, // 标记
                superscript: true, // 上角标
                subscript: true, // 下角标
                quote: false, // 引用
                ol: true, // 有序列表
                ul: true, // 无序列表
                link: true, // 链接
                imagelink: false, // 图片链接
                code: false, // code
                table: true, // 表格
                fullscreen: true, // 全屏编辑
                readmodel: false, // 沉浸式阅读
                htmlcode: false, // 展示html源码
                help: true, // 帮助
                /* 1.3.5 */
                undo: true, // 上一步
                redo: true, // 下一步
                trash: true, // 清空
                save: false, // 保存（触发events中的save事件）
                /* 1.4.2 */
                navigation: false, // 导航目录
                /* 2.1.8 */
                alignleft: true, // 左对齐
                aligncenter: true, // 居中
                alignright: true, // 右对齐
                /* 2.2.1 */
                subfield: false, // 单双栏模式
                preview: true // 预览
            },
            showTag: true,
            tid2tname: {}
        }
    },
    methods: {
        submitTitle() {
            this.step = 2;
        },
        submitTags() {
            this.step = 3;
        },
        initTagState() {
            let tagList = this.$store.state.tagList;
            for (let category in tagList) {
                for (let tag in tagList[category]) {
                    this.tagState[tagList[category][tag]] = false;
                    if (category === 'Env') {
                        this.tags['环境'][tag] = tagList[category][tag];
                    }
                    else if (category === 'Lang') {
                        this.tags['语言'][tag] = tagList[category][tag];
                    }
                    else {
                        this.tags['其它'][tag] = tagList[category][tag];
                    }
                    this.tid2tname[tagList[category][tag]] = tag;
                }
            }
        },
        select(tid) {
            this.tagState[tid] = !this.tagState[tid];
            this.showTag = false;
            this.showTag = true;
            let selectedTag = [];
            for (let id in this.tagState) {
                if (this.tagState[id]) {
                    selectedTag.push(id);
                }
            }
            this.selectedTag = selectedTag;
        },
        submitQuestion() {
            let _this = this;
            this.$axios.post(this.BASE_URL + '/api/questions/add_question', {
                title: _this.title,
                remarks: _this.detail,
                tags: _this.selectedTag
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(function (response) {
                    _this.$store.state.mobileStatus = 'questionList';
                })
            this.step = 1;
        }
    },
    computed: {
        prop() {
            let data = {
                subfield: false,// 单双栏模式
                defaultOpen: 'edit',//edit： 默认展示编辑区域 ， preview： 默认展示预览区域
                editable: true,
                toolbarsFlag: true,
                scrollStyle: false,
                boxShadow: true//边框
            };
            return data;
        }
    },
    created() {
        this.initTagState();
    }
}
</script>

<style scoped>

/*.shell {*/
/*    position: relative;*/
/*    left: 0vw;*/
/*    top: 0;*/
/*    width: 100vw;*/
/*    height: 60vh;*/
/*    !*background-color: rgba(246, 246, 246, 1);*!*/
/*    !*flex-direction: column;*!*/
/*    !*align-items: stretch;*!*/
/*    padding: 0 0;*/
/*}*/

/*.el-aside {*/
/*    margin-left: 50px;*/
/*    padding: 20px 0;*/
/*}*/

.main-body {
    flex-direction: column;
    align-items: stretch;
    margin-right: 0;
}

.el-main {
    flex-direction: column;
    align-items: stretch;
    justify-content: flex-start;
}

.el-header {
    border-bottom: 1px solid #eaecf1;
    width: 100vw;
    align-items: center;
    padding: 5%;
    /*height: 5vh;*/
}

.el-main > * {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    margin: 0 0px;
}

.el-button {
    align-self: flex-end;
    margin-top: 2vh;
    float: right;
}

h1 {
    margin: 0;
    font-size: 20px;
}

.edit-title {
    margin-top: 5vh;
}

.title {
    margin-top: 3vh;
    margin-left: 5vw;
}

.select-tag {
    margin-top: 3vh;
}

.tag-type {
    flex-direction: row;
    margin-bottom: 2vh;
}

.tag {
    flex-direction: row;
    margin-top: 2vh;
    margin-left: 5vw;
}

.tag .el-tag {
    cursor: default;
}

.el-tag {
    margin-right: 2vw;
}

.edit-detail {
    margin-top: 3vh;
}

.el-footer {
    /*position: absolute;*/
    margin-top: 2vh;
    width: 100vw;
}

.selector {
    cursor: pointer;
    margin-right: 2vw;
}

/*.markdown-body {*/
/*    min-height: 50vh;*/
/*    max-height: 50vh;*/
/*}*/

/*.fullscreen {*/
/*    min-height: 100vh;*/
/*    max-height: 100vh;*/
/*}*/

</style>
