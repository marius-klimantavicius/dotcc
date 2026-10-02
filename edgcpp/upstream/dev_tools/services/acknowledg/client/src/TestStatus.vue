<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import {
    computed,
    inject,
    nextTick,
    onMounted,
    watch
  } from 'vue'

  import { computeGradientStyleForConfigs } from './config-color.js'

  const emit = defineEmits(['status-clicked'])
  const props = defineProps(['test', 'statusInfo'])

  const activeTestName = inject('activeTestName')
  const activeDiffHash = inject('activeDiffHash')
  const multiConfig = inject('multiConfig')
  const statusListSelection = inject('statusListSelection')

  const groupContainerEls = new Map()
  const isActive = computed(() => activeTestName.value === props.test)

  const setGroupContainerEl = (diffHash, el) => {
    if (el) {
      groupContainerEls.set(diffHash, el)
    } else {
      groupContainerEls.delete(diffHash)
    }
  }

  const scrollIfActive = async () => {
    if (!isActive.value) {
      return
    }
    await nextTick()
    groupContainerEls.get(activeDiffHash.value)?.scrollIntoView({
      behavior: 'smooth',
      block: 'center'
    })
  }

  onMounted(scrollIfActive)
  watch([isActive, activeDiffHash, statusListSelection], scrollIfActive)

  const qualifierColorClass = (qualifier) => {
    if (qualifier === '+') {
      return 'green-badge'
    }
    if (qualifier === '-') {
      return 'red-badge'
    }
    return null
  }

  const statusColorClass = (qualifier) => {
    if (qualifier.indexOf('PASS') !== -1) {
      return 'green-badge'
    }
    if (qualifier.indexOf('FAIL') !== -1 ||
        qualifier.indexOf('ABORT') !== -1 ||
        qualifier.indexOf('BADC') !== -1 ||
        qualifier.indexOf('CATASTROPHE') !== -1) {
      return 'red-badge'
    }
    return null
  }

  const styleForGroup = (group) => {
    const muted = !isActive.value || group.diff_hash !== activeDiffHash.value
    return {
      background: computeGradientStyleForConfigs(group.configs, { muted })
    }
  }

  const emitStatusClick = (group) => {
    if (group) {
      emit('status-clicked', props.test, group)
    } else {
      if (isActive.value) {
        return
      }
      emit('status-clicked', props.test, props.statusInfo.groups[0])
    }
  }
</script>

<template>
  <div class="test-status-container"
       :class="{ 'active-test': isActive }"
       @click.self="emitStatusClick()">
    <div @click.self="emitStatusClick()" class="test-status-inner-container">
      <div @click.self="emitStatusClick()"
           class="test-name test-label-text"
           :title="props.test">
        {{ props.test }}
      </div>
      <div v-if="props.statusInfo.remark" @click.self="emitStatusClick()"
           class="test-remark test-label-text">
        {{ props.statusInfo.remark }}
      </div>
      <div v-for="group in props.statusInfo.groups"
           :key="group.diff_hash"
           :ref="(el) => setGroupContainerEl(group.diff_hash, el)"
           class="config-status-container"
           :class="{
             'active-cfg': isActive && group.diff_hash === activeDiffHash
           }"
           :style="styleForGroup(group)"
           @click="emitStatusClick(group)">
        <div class="status-line-container">
          <div v-for="(lineInfo, lineIndex) in group.lines"
               :key="lineIndex"
               class="status-line"
               :class="qualifierColorClass(lineInfo.qualifier)">
            <div class="test-qualifier-container">
              <div class="test-qualifier">{{ lineInfo.qualifier }}</div>
            </div>
            <div :class="statusColorClass(lineInfo.status)"
                  class="status-line-highlight">
              <span class="status-line-variant"
                    :title="lineInfo.variant">{{ lineInfo.variant }}</span>
              <span>:</span>
              <span class="status-line-status">{{ lineInfo.status }}</span>
            </div>
          </div>
        </div>
        <div v-if="multiConfig" class="group-config-names">
          <span v-for="config in group.configs"
                :key="config"
                class="group-config-name">{{ config }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .test-name {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .test-remark {
    padding: 3px;
    border-radius: 5px 8px 5px 2px;
    margin: 3px;
    white-space: wrap;
  }
  .test-qualifier-container {
    display: flex;
    align-items: center;
  }
  .test-qualifier {
    height: 16px;
    width: 16px;
    line-height: 16px;
    font-size: 12px;
    font-weight: bold;
    text-align: center;
    border-radius: 8px;
  }
  .test-status-container {
    padding-left: 5px;
    padding-right: 5px;
    padding-top: 4px;
    padding-bottom: 4px;
  }
  .test-status-inner-container {
    padding: 3px;
    border-radius: 3px;
  }
  .config-status-container {
    margin-top: 3px;
    margin-bottom: 3px;
    padding: 4px;
    border-radius: 3px;
  }
  .group-config-names {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
    margin-top: 4px;
  }
  .group-config-name {
    font-size: 0.85em;
  }
  .status-line-container {
    border-radius: 3px;
    font-family: monospace;
    padding: 1px;
    margin-bottom: 3px;
  }
  .status-line {
    margin: 4px;
    padding: 4px;
    border-radius: 3px;
    font-family: monospace;
    display: flex;
    flex-direction: row;
    gap: 6px;
  }
  .status-line-highlight {
    padding: 2px;
  }
  .status-line-variant {
    overflow: clip;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 30em;
    display: inline-block;
  }
  .status-line-status {
    font-weight: bold;
    display: inline-block;
  }
  .active-cfg.config-status-container {
    color: white;
  }
  .green-badge {
    color: #618555;
  }
  .red-badge {
    color: #d1173e;
  }
  .green-badge .test-qualifier {
    background-color: #618555;
    color: white;
  }
  .red-badge .test-qualifier {
    background-color: #d1173e;
    color: black;
  }
  .active-cfg .green-badge {
    background-color: #618555;
    color: white;
    border-radius: 3px;
  }
  .active-cfg .red-badge {
    background-color: #d1173e;
    color: white;
    border-radius: 3px;
  }
  @media (prefers-color-scheme: light) {
    .green-badge .test-qualifier {
      color: white;
    }
    .red-badge .test-qualifier {
      color: white;
    }
    .active-cfg .green-badge .test-qualifier {
      background-color: white;
      color: #618555;
    }
    .active-cfg .red-badge .test-qualifier {
      background-color: white;
      color: #d1173e;
    }
    .test-label-text {
      color: rgb(0, 0, 0, .8);
    }
    .test-remark {
      background-color: #efefef;
    }
    .test-status-inner-container {
      background-color: #cbcbcb;
      color: black;
    }
    .config-status-container {
      color: rgb(255, 255, 255, .8);
    }
    .status-line-container {
      background: rgb(255, 255, 255, .4);
    }
    .status-line {
      background: rgb(255, 255, 255, .9);
    }
    .active-test .test-remark {
      background-color: white;
      color: black;
    }
    .active-test .test-name {
      color: white;
    }
    .active-test .test-status-inner-container {
      background-color: #555555;
      color: white;
    }
    .active-cfg .status-line-container {
      background: rgb(255, 255, 255, .9);
    }
  }
  @media (prefers-color-scheme: dark) {
    .green-badge .test-qualifier {
      color: black;
    }
    .red-badge .test-qualifier {
      color: black;
    }
    .active-cfg .green-badge .test-qualifier {
      background-color: black;
      color: #618555;
    }
    .active-cfg .red-badge .test-qualifier {
      background-color: black;
      color: #d1173e;
    }
    .test-label-text {
      color: rgb(255, 255, 255, .6);
    }
    .test-remark {
      background-color: #111111;
    }
    .test-status-inner-container {
      background-color: #252525;
      color: white;
    }
    .config-status-container {
      color: rgb(255, 255, 255, .5);
    }
    .status-line-container {
      background: rgb(0, 0, 0, .4);
    }
    .status-line {
      background: black;
    }
    .active-test .test-label-text {
      color: white;
    }
    .active-test .test-status-inner-container {
      background-color: #484848;
      color: white;
    }
    .active-cfg .status-line-container {
      background: rgb(0, 0, 0, .9);
    }
  }
</style>
