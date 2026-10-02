<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import { ref } from 'vue'

  const isActive = ref(false)

  setTimeout(() => {
    isActive.value = true
  }, 100)
</script>

<template>
  <span class="loader" :style="{ opacity: (isActive ? 1 : 0) }"></span>
</template>

<style scoped>
  .loader {
    width: 1.2em;
    height: 1.2em;
    border: 3px dotted green;
    border-style: solid solid dotted dotted;
    border-radius: 50%;
    display: inline-block;
    position: relative;
    box-sizing: border-box;
    animation: rotation 2s linear infinite;
  }
  .loader::after {
    content: '';
    box-sizing: border-box;
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    margin: auto;
    border: 3px dotted #FF3D00;
    border-style: solid solid dotted;
    width: .8em;
    height: .8em;
    border-radius: 50%;
    animation: rotationBack 1s linear infinite;
    transform-origin: center center;
  }

  @keyframes rotation {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
  @keyframes rotationBack {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(-360deg);
    }
  }
</style>


