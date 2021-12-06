<template>
    <el-container class="shell">
        <el-header>
            AIApe
        </el-header>
        <el-container class="main-body">
            <el-steps :active="step" align-center>
                <el-step title="问题标题" @click.native="gotoStep(1)"
                         style="cursor: pointer"></el-step>
                <el-step title="选择标签" @click.native="gotoStep(2)"
                         style="cursor: pointer"></el-step>
                <el-step title="问题详情" @click.native="gotoStep(3)"
                         style="cursor: pointer"></el-step>
            </el-steps>
            <el-main>
                <el-button type="primary" @click="getTemplate">获取问题模板</el-button>
                <transition name="slide" enter-active-class="slideInUp" leave-active-class="none">
                    <div class="edit-title" v-show="step === 1">
                        <el-input v-show="step === 1" v-model="title"></el-input>
                        <el-button v-show="step === 1" @click="submitTitle" type="primary">提交</el-button>
                    </div>
                </transition>
                <transition name="fade">
                    <div class="title" v-show="step > 1">
                        <h1>{{ title }}</h1>
                    </div>
                </transition>
                <transition name="slide" enter-active-class="slideInUp" leave-active-class="none">
                    <div class="select-tag" v-show="step === 2">
                        <div v-for="(ls, type) in tags" class="tag-type">
                            <span>{{ type }}</span>
                            <el-tag class="selector" v-for="(tid, tname) in ls" :key="tid"
                                    :effect="tagState[tid]? 'dark' : 'light'" @click="select(tid, tname)"
                                    v-show="showTag">
                                {{ tname }}
                            </el-tag>
                        </div>
                        <el-button type="primary" @click="submitTags">完成</el-button>
                    </div>
                </transition>
                <transition name="fade">
                    <div class="tag" v-show="step > 2">
                        <el-tag v-for="tid in selectedTag" :key="tid">{{ tid2tname[tid] }}</el-tag>
                    </div>
                </transition>
                <transition name="slide" enter-active-class="slideInUp" leave-active-class="slideOutDown">
                    <div class="edit-detail" v-show="step === 3">
                        <mavon-editor :toolbars="toolbars" v-model="detail" ref=md
                                      :subfield="prop.subfield" :defaultOpen="prop.defaultOpen"
                                      :toolbarsFlag="prop.toolbarsFlag" :editable="prop.editable"
                                      :scrollStyle="prop.scrollStyle" :boxShadow="prop.boxShadow"
                                      placeholder="详细描述你的问题...">

                        </mavon-editor>
                        <el-button type="primary" @click="submitQuestion">提交</el-button>
                    </div>
                </transition>
            </el-main>
        </el-container>
    </el-container>
</template>

<script>
export default {
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
            if (this.title === '') {
                this.$message({
                    message: '问题标题不能为空',
                    type: 'warning'
                })
                return;
            }
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
                    } else if (category === 'Lang') {
                        this.tags['语言'][tag] = tagList[category][tag];
                    } else {
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
            if (this.detail === '') {
                this.$message({
                    message: '问题详情不能为空',
                    type: 'warning'
                });
                return;
            }
            this.$confirm('是否提交你的问题？', '提交确认', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
            }).then(() =>
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
                        _this.title = '';
                        _this.detail = '';
                        _this.selectedTag = [];
                        _this.step = 1;
                        _this.initTagState();
                        _this.$message({
                            message: '提问成功',
                            type: 'success'
                        })
                        _this.$changePage(2);
                    })
            )
                .catch(() => {
                    this.$message({
                        type: 'info',
                        message: '取消提交'
                    });
                })
        },
        gotoStep(step) {
            if (step > 1 && this.title === '') {
                return;
            }
            this.step = step;
        },
        getTemplate() {
            if (this.$store.state.template.title === '') {
                this.$message({
                    message: '没有可用的模板',
                    type: 'warning'
                })
                return;
            }
            let _this = this;
            this.$confirm('获取问题模板将清空当前内容，是否继续？', '操作确认', {
                cancelButtonText: '取消',
                confirmButtonText: '确定',
            })
            .then(() => {
                if (_this.$store.state.template.title !== '') {
                    _this.title = _this.$store.state.template.title;
                }
                _this.initTagState();
                for (let tid of _this.$store.state.template.tags) {
                    _this.tagState[tid] = true;
                }
                _this.selectedTag = _this.$store.state.template.tags;
                if (_this.$store.state.template.remarks !== '' && _this.$store.state.template.remarks !== null) {
                    _this.detail = _this.$store.state.template.remarks;
                }
                _this.step = 3;
            })

        }
    },
    computed: {
        prop() {
            return {
                subfield: false,// 单双栏模式
                defaultOpen: 'edit',//edit： 默认展示编辑区域 ， preview： 默认展示预览区域
                editable: true,
                toolbarsFlag: true,
                scrollStyle: false,
                boxShadow: true//边框
            };
        }
    },
    created() {
        this.initTagState();
    }
}
</script>

<style scoped>
.shell {
    position: absolute;
    left: 5vw;
    top: 0;
    width: 95vw;
    height: 100vh;
    background-color: rgba(246, 246, 246, 0.5);
    flex-direction: column;
    align-items: stretch;
    padding: 50px 0;
}

.el-aside {
    margin-left: 50px;
    padding: 20px 0;
}

.main-body {
    flex-direction: column;
    align-items: stretch;
}

.el-main {
    flex-direction: column;
    align-items: stretch;
    justify-content: flex-start;
}

.el-header {
    font-size: 20px;
    font-weight: bold;
    align-self: center;
}

.el-main > * {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    margin: 0 200px;
}

.el-button {
    align-self: flex-end;
    margin-top: 10px;
}

h1 {
    margin: 0;
    font-size: 20px;
}

.edit-title {
    margin-top: 20px;
}

.title {
    margin-top: 20px;
}

.select-tag {
    margin-top: 20px;
}

.tag-type {
    flex-direction: row;
    margin-bottom: 20px;
}

.tag {
    flex-direction: row;
    margin-top: 20px;
    flex-wrap: wrap;
}

.tag .el-tag {
    cursor: default;
    margin-top: 10px;
}

.el-tag {
    margin-right: 10px;
}

.edit-detail {
    margin-top: 20px;
}

.markdown-body {
    min-height: 50vh;
    max-height: 50vh;
}

.fullscreen {
    min-height: 100vh;
    max-height: 100vh;
}

::-webkit-scrollbar {
    width: 0;
}

.selector {
    cursor: pointer;
}
</style>
