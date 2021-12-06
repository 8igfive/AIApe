<template>
    <el-container>
        <el-header height=5vh>
            AIApe
        </el-header>
        <el-main style="position: fixed; top: 5vh; bottom: 5vh;">
            <el-header v-if="analysis" height=5vh class="back">
                <el-link v-on:click="goBack">返回</el-link>
            </el-header>
            <el-container style="flex-direction: column;" v-if="analysis">
                <h1>问题列表</h1>
                <el-container class="issues">
                    <el-main v-for="issue in issues" :key="issue.desc" class="issue">
                        {{ issue.desc }}
                    </el-main>
                </el-container>
            </el-container>
            <el-container v-else class="code">
                <!--            <codemirror style="width: 50vw;" v-if="show" v-model="code" :options="cmOptions"-->
                <!--                        class="editor"></codemirror>-->
                <!--            <div style="height: 90vh" v-else></div>-->
                <div>
                    <el-button style="float: left" id="submit-button" type="primary" size="small" @click="submitCode">
                        Format&Check
                    </el-button>
                </div>
                <codemirror style="width: 100vw; z-index: 1" v-if="show" v-model="code" :options="cmOptions"
                            class="editor"></codemirror>
            </el-container>
        </el-main>
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
    name: "MobileCodeAnalysis",
    components: {
        codemirror
    },
    data() {
        return {
            analysis: false,
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
        goBack() {
            this.analysis = false;
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
                    _this.code = response.data.fmtCode;
                    _this.issues = response.data.messages;
                    _this.analysis = true;
                })
        }
    },
    mounted() {
    }
}
</script>

<style scoped>

.el-header {
    position: fixed;
    border-bottom: 1px solid #eaecf1;
    width: 100vw;
    align-items: center;
    padding: 5%;
    /*height: 5vh;*/
}

.code {
    flex-direction: column;
    /*height: 0vh;*/
    background-color: white;
    margin-right: 0;
    align-items: stretch;
    flex-grow: 1;
    justify-content: space-between;
}

.editor {
    width: 100vw;
}

.back {
    border-bottom: 0;
    width: 100vw;
    align-items: center;
    padding: 5%;
}

.issues {
    width: 100vw;
    height: 50vh;
    flex-grow: 0;
    margin-left: 0px;
    flex-direction: column;
    align-items: center;
    justify-content: space-around;
}

.issue {
    box-shadow: 0 2px 5px 0 rgba(0, 0, 0, 0.1);
    background-color: white;
    width: 100vw;
    margin-bottom: 10px;
    border-radius: 2px;
    padding: 10px;
}

</style>
