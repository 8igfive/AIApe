<template>
    <el-container class="shell">
        <el-header class="header">
            数据统计
        </el-header>
        <el-header>
            详细统计报告在<a href="/static/AIApe-Data-Statistics-Report.pdf" download="AIApe-Data-Statistics-Report.pdf">此处</a>下载
        </el-header>
        <el-main class="main">
            <div style="text-align: center">
                <div class="summary-image">
                    <div class="image-block" v-for="(path, i) in summaryPath" :key="path">
                        <span class="image-title">{{ summaryTitle[i] }}</span>
                        <el-image class="image" :src="path" fit="fill"></el-image>
                    </div>
                </div>
                <div class="title">
                    环境标签相关统计数据
                </div>
                <div class="button-block">
                    <el-button v-for="(tid, tname) in this.$store.state.tagList.Env" :key="tid" @click="envClick(tname)">
                        {{ tname }}
                    </el-button>
                </div>
                <div class="summary-image">
                    <div class="image-block" v-for="(path, i) in envPath" :key="path">
                        <span class="image-title">{{ envTitle[i] }}</span>
                        <el-image class="image" :src="path" fit="fill"></el-image>
                    </div>
                </div>
                <div class="title">
                    语言标签相关统计数据
                </div>
                <div class="button-block">
                    <el-button v-for="(tid, tname) in this.$store.state.tagList.Lang" :key="tid" @click="langClick(tname)">
                        {{ tname }}
                    </el-button>
                </div>
                <div class="summary-image">
                    <div class="image-block" v-for="(path, i) in langPath" :key="path">
                        <span class="image-title">{{ langTitle[i] }}</span>
                        <el-image class="image" :src="path" fit="fill"></el-image>
                    </div>
                </div>
                <div class="title">
                    其他标签相关统计数据
                </div>
                <div class="button-block">
                    <el-button v-for="(tid, tname) in this.$store.state.tagList.Other" :key="tid" @click="otherClick(tname)">
                        {{ tname }}
                    </el-button>
                </div>
                <div class="summary-image">
                    <div class="image-block" v-for="(path, i) in otherPath" :key="path">
                        <span class="image-title">{{ otherTitle[i] }}</span>
                        <el-image class="image" :src="path" fit="fill"></el-image>
                    </div>
                </div>
            </div>
        </el-main>
    </el-container>
</template>

<script>
export default {
    components: {
    },
    data() {
        return {
            path: "../../assets/statistics/",
            tname_dict: {
                "标准库": "standard_lib",
                "代码": "code",
                "第三方库": "third_lib",
                "工具使用": "tool_use",
                "关键字": "keyword",
                "环境配置": "env_config",
                "算法": "algo",
                "网络": "network",
                "硬件": "hardware",
                "语句": "sentence",
                "C语言": "C",
            },
            summaryPath: [
                require("../../assets/statistics/summary_Env_bar.png"),
                require("../../assets/statistics/summary_Lang_bar.png"),
                require("../../assets/statistics/summary_Other_bar.png"),
            ],
            summaryTitle: [
                "不同环境问题数量",
                "不同语言问题数量",
                "其他问题数量",
            ],
            envPath: [
                require("../../assets/statistics/detail_Linux_title_length_kde.png"),
                require("../../assets/statistics/detail_Linux_remarks_length_kde.png"),
                require("../../assets/statistics/detail_Linux_tags_dist_bar.png"),
            ],
            envTitle: [
                "Linux问题标题长度",
                "Linux问题描述长度",
                "Linux问题相关标签",
            ],
            langPath: [
                require("../../assets/statistics/detail_C_title_length_kde.png"),
                require("../../assets/statistics/detail_C_remarks_length_kde.png"),
                require("../../assets/statistics/detail_C_tags_dist_bar.png"),
            ],
            langTitle: [
                "C语言问题标题长度",
                "C语言问题描述长度",
                "C语言问题相关标签",
            ],
            otherPath: [
                require("../../assets/statistics/detail_env_config_title_length_kde.png"),
                require("../../assets/statistics/detail_env_config_remarks_length_kde.png"),
                require("../../assets/statistics/detail_env_config_tags_dist_bar.png"),
            ],
            otherTitle: [
                "环境配置问题标题长度",
                "环境配置问题描述长度",
                "环境配置问题相关标签",
            ],
        }
    },
    methods: {
        envClick(tname) {
            this.envTitle = [
                tname + "问题标题长度",
                tname + "问题描述长度",
                tname + "问题相关标签",
            ];
            if (tname in this.tname_dict) {
                tname = this.tname_dict[tname]
            }
            this.envPath = [
                require("../../assets/statistics/detail_" + tname + "_title_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_remarks_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_tags_dist_bar.png"),
            ];
        },
        langClick(tname) {
            this.langTitle = [
                tname + "问题标题长度",
                tname + "问题描述长度",
                tname + "问题相关标签",
            ];
            if (tname in this.tname_dict) {
                tname = this.tname_dict[tname]
            }
            this.langPath = [
                require("../../assets/statistics/detail_" + tname + "_title_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_remarks_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_tags_dist_bar.png"),
            ];
        },
        otherClick(tname) {
            this.otherTitle = [
                tname + "问题标题长度",
                tname + "问题描述长度",
                tname + "问题相关标签",
            ];
            if (tname in this.tname_dict) {
                tname = this.tname_dict[tname]
            }
            this.otherPath = [
                require("../../assets/statistics/detail_" + tname + "_title_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_remarks_length_kde.png"),
                require("../../assets/statistics/detail_" + tname + "_tags_dist_bar.png"),
            ];
        },
    },
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
    /* border: red 2px solid; */
}

.header {
    line-height: 60px;
    font-size: 32px;
}

.title {
    line-height: 48px;
    font-size: 24px;
}

.main {
    /* border: 1px red solid; */
    width: 100%;
}

.summary-image {
    display: flex;
    height: 300px;
    align-items: center;
    width: 100%;
    margin: 10px 0 20px 0;
}

.image-block {
    margin:0px 20px;
    text-align: center;
}

.image-title {
    font-size: 16px;
    line-height: 16px;
}

.image {
    margin: 5px;
}

.button-block {
    margin-bottom: 20px;
}
</style>
