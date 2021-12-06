<template>
    <div>
        <div id="page1">
            <vue-particles
                color="#fff"
                class="background"
                :particleOpacity="0.7"
                :particlesNumber="60"
                shapeType="triangle"
                :particleSize="4"
                linesColor="#fff"
                :linesWidth="1"
                :lineLinked="true"
                :lineOpacity="0.4"
                :linesDistance="150"
                :moveSpeed="1"
                :hoverEffect="true"
                hoverMode="grab"
                :clickEffect="false"
                clickMode="push"
            >
            </vue-particles>
            <div class="title">
                AIApe 2.0
            </div>
            <div class="subtitle">
                全新设计 为你而来
            </div>
            <i class="el-icon-arrow-down" id='arrow-down' @click="gotoIntro"></i>
        </div>
        <div id="page2">
            <el-carousel indicator-position="outside" height="85vh">
                <el-carousel-item v-for="item in 4" :key="item">
                    <h3>{{ item }}</h3>
                </el-carousel-item>
            </el-carousel>
            <el-button id="gotoUse" @click="gotoUse">开始使用</el-button>
        </div>
    </div>
</template>

<script>
import $ from 'jquery';

export default {
    data() {
        return {
            bgSrc: require('../../assets/background2.jpg')
        }
    },
    methods: {
        gotoIntro() {
            $('html, body').animate({scrollTop: window.innerHeight}, 500);
        },
        gotoUse() {
            let path = this.$router.resolve({
                path: '/chat'
            })
            window.open(path.href, '_blank');
        }
    }
}
</script>

<style scoped>
.background {
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
    position: absolute;
    user-select: none;
    background: linear-gradient(to right bottom, #005aa7, #fffde4);
}

#page1 {
    display: flex;
    width: 100vw;
    height: 100vh;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

#page2 {
    display: flex;
    width: 100vw;
    height: 100vh;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: linear-gradient(to right top, #005aa7, #fffde4);
}

.title {
    font-family: pinghei, serif;
    font-size: 120px;
    font-weight: normal;
}

.subtitle {
    font-family: pinghei, serif;
    font-size: 80px;
    margin-top: 20px;
    font-weight: lighter;
}

.title, .subtitle {
    cursor: default;
    user-select: none;
    color: aliceblue;
}

.el-icon-arrow-down {
    position: absolute;
    bottom: 0;
    font-size: 60px;
    color: aliceblue;
    font-weight: 100;
    opacity: 0.5;
    cursor: pointer;
}

.el-icon-arrow-down:hover {
    opacity: 1;
}

.el-carousel__button {
    background-color: white;
}


.el-carousel {
    width: 100vw;
}

#gotoUse {
    border-radius: 0;
    background-color: transparent;
    font-family: pinghei, serif;
    color: aliceblue;
    margin-top: 20px;
    font-size: 25px;
    font-weight: lighter;
    width: 150px;
}
</style>
