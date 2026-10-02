<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import { defineProps } from 'vue'

  const props = defineProps(["message", "type"])
</script>

<template>
  <div :class="props.type">{{ props.message }}</div>
</template>

<style scoped>
  div {
    display: block;
    min-width: 250px;
    border-radius: 15px;
    padding: 10px;
    margin: 10px;
    opacity: 0;
    animation-duration: 3s;
    animation-name: appear-and-then-vanish;
  }
  .success {
    background-color: green;
    color: white;
  }
  .error {
    background-color: red;
    color: white;
  }
  @keyframes appear-and-then-vanish {
    0% {
      opacity: 0;
    }
    20%, 80% {
      opacity: .8;
    }
    100% {
      opacity: 0;
    }
  }
</style>

