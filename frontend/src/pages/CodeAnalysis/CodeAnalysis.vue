<template>
    <el-container class="shell">
        <el-container class="code">
            <el-header>
                AIApe
            </el-header>
            <el-main class="selector">
                <span>代码分析器</span>
            </el-main>
            <codemirror ref="codeBox" style="width: 50vw;" v-if="show" v-model="code" :options="cmOptions"
                        class="editor"></codemirror>
            <div style="height: 90vh" v-else></div>
            <div class="tip-submit">
                <i class="el-icon-info">可以直接拖拽源代码文件至文本框噢~</i>
                <el-button id="submit-button" type="primary" @click="submitCode">提交</el-button>
            </div>
        </el-container>
        <el-container class="issues">
            <div>
                <el-main class="issue" v-for="issue in issues" :key="issue.desc" :class="issue.level">
                    <div>
                        {{ issue.desc }}
                    </div>
                    <el-link id="position" :underline="false"
                             @click="gotoPosition(issue.line - 1, issue.column - 1)">{{
                            issue.line
                        }}:{{ issue.column }}
                    </el-link>
                </el-main>
            </div>
            <el-main class="info">
                <span>AIApe</span>
                <span>京ICP备 2021007509号-1</span>
                <span>
                联系我们 @2021软件工程DQSJ
            </span>
            </el-main>
        </el-container>
    </el-container>
</template>

<script>
import {codemirror} from 'vue-codemirror';
import 'codemirror/theme/base16-light.css'
import 'codemirror/theme/idea.css';
import 'codemirror/lib/codemirror.css';
import 'codemirror/lib/codemirror.js';
import 'codemirror/mode/clike/clike.js';
import 'codemirror/addon/display/autorefresh.js';
import 'codemirror/addon/edit/closebrackets.js';
import 'codemirror/addon/edit/matchbrackets.js';
import 'codemirror/addon/wrap/hardwrap.js';
import 'codemirror/addon/fold/foldcode.js';
import 'codemirror/addon/fold/foldgutter.js';
import 'codemirror/addon/fold/brace-fold.js';
import 'codemirror/addon/fold/comment-fold.js';

export default {
    components: {
        codemirror
    },
    data() {
        return {
            show: true,
            state: 'input',
            code: '',
            cmOptions: {
                value: '',
                mode: 'text/x-csrc',
                theme: "idea",
                lineNumbers: true,
                line: true,
                indentUnit: 4,
                matchBrackets: true,
                autoCloseBrackets: true,
                autoRefresh: true
            },
            issues: []
        }
    },
    computed: {
        showEditor() {
            return this.$store.state.codeAnalysis.showEditor;
        }
    },
    methods: {
        html2Escape(sHtml) {
            return sHtml.replace(/[<>&"]/g, function (c) {
                return {'<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;'}[c];
            });
        },
        return2Br(str) {
            return str.replace(/\r?\n/g, "<br />");
        },
        submitCode() {
            let _this = this;
            this.$axios.post(this.BASE_URL + '/api/code/static_analyze', {
                code: _this.code
            }, {
                headers: {
                    Authorization: 'Bearer ' + _this.$store.state.token,
                    type: 'application/json;charset=utf-8'
                }
            })
                .then(function (response) {
                    console.log(response);
                    _this.code = response.data.fmtCode;
                    _this.issues = response.data.messages;
                })
        },
        gotoPosition(line, column) {
            this.$refs['codeBox'].codemirror.focus();
            this.$refs['codeBox'].codemirror.setCursor(line, column);
        }
    },
    mounted() {
        this.show = false;
        setTimeout(() => {
            this.show = true;
        }, 800);
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
    padding-left: 100px;
    padding-right: 100px;
    background-color: rgba(246, 246, 246, 0.5);
}

.code {
    flex-direction: column;
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    border-radius: 2px;
    height: 95vh;
    background-color: white;
    margin-right: 5px;
    align-items: stretch;
    flex-grow: 1;
    justify-content: flex-start;
}

.editor {
    width: 50vw;
    border-bottom: 1px solid lightgrey;
    font-size: 20px;
}

.issues {
    width: 30vw;
    border-radius: 2px;
    height: 95vh;
    flex-grow: 0;
    margin-left: 10px;
    flex-direction: column;
    align-items: center;
    justify-content: space-between;
}

.issue {
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    background-color: white;
    width: 30vw;
    margin-bottom: 10px;
    border-radius: 2px;
    padding: 10px;
    flex-direction: column;
    justify-content: space-between;
    flex-grow: 0;
}

.tip-submit {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-left: 10px;
    margin-top: 1px;
}

.info {
    box-shadow: 0 0 0 0;
    background-color: rgba(0, 0, 0, 0);
    flex-direction: column;
    align-items: center;
    flex-grow: 0;
    justify-self: flex-end;
}

.error {
    background-color: #fef0f0;
}

.warning {
    background-color: #fdf6ec;
}

.style {
    background-color: #ecf5ff;
}

.performance {
    background-color: #f0f9eb;
}

.portability {
    background-color: #f4f4f5;
}

.issue > * {
    display: flex;
}

#position {
    align-self: flex-end;
}

#position {
    color: #409eff;
}

.el-header {
    padding-top: 10px;
    font-size: 30px;
    justify-content: center;
}

.selector {
    flex: none;
    align-self: stretch;
    padding-left: 10px;
    border-bottom: 1px solid lightgrey;
    padding-bottom: 10px;
}

</style>
