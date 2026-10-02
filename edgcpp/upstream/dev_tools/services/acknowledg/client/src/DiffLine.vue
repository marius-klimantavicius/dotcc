<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import { defineProps } from 'vue'

  const props = defineProps(["lineText"])

  const colorClass = (line) => {
    if (line.startsWith("#")) {
      return "comment"
    } else if (line.startsWith("+")) {
      return "addition"
    } else if (line.startsWith("-")) {
      return "deletion"
    }
    return '';
  }
</script>

<template>
  <pre :class="colorClass(props.lineText)">{{ props.lineText }}</pre>
</template>

<style scoped>
  .addition {
    background-color: #618555;
    color: white;
  }
  .deletion {
    background-color: #d1173e;
    color: white;
  }
  .comment {
    background-color: cyan;
  }
  pre {
    margin: 0px;
  }
</style>
