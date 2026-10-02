<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import { onMounted, onUnmounted, useTemplateRef } from 'vue'

  const rootRef = useTemplateRef('root')

  const keyListener = (event) => {
    if (event.key !== 'PageUp' && event.key !== 'PageDown' &&
        event.key !== 'ArrowUp' && event.key !== 'ArrowDown' &&
        event.key !== 'Home' && event.key !== 'End' &&
        event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') {
      return
    }

    event.preventDefault()
    if (!rootRef.value) {
      return
    }

    const isProgressDirection = (
      event.key === 'PageDown' ||
      event.key === 'ArrowDown' ||
      event.key === 'End' ||
      event.key === 'ArrowRight'
    )

    let scrollUnit = 0
    let isScrollVertical = true
    if (event.key === 'PageUp' || event.key === 'PageDown') {
      // If this is a PageUp or PageDown use half of the visible region.
      scrollUnit = rootRef.value.clientHeight / 2
    } else if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      // If this is a up or down arrow use a quarter of the visible region.
      scrollUnit = rootRef.value.clientHeight / 4
    } else if (event.key === 'Home' || event.key === 'End') {
      // If this is a home or end key use a half of the visible region
      // horizontally.
      scrollUnit = rootRef.value.clientWidth / 2
      isScrollVertical = false
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      // If this is a left or right arrow use a quarter of the visible
      // region horizontally.
      scrollUnit = rootRef.value.clientWidth / 4
      isScrollVertical = false
    }

    let scrollTop = rootRef.value.scrollTop
    let scrollLeft = rootRef.value.scrollLeft
    if (isScrollVertical) {
      if (isProgressDirection) {
        scrollTop += scrollUnit
      } else {
        scrollTop -= scrollUnit
      }
    } else if (isProgressDirection) {
      scrollLeft += scrollUnit
    } else {
      scrollLeft -= scrollUnit
    }

    rootRef.value.scrollTo({
      top: scrollTop,
      left: scrollLeft,
      behavior: 'smooth'
    })
  }

  onMounted(() => {
    document.addEventListener('keydown', keyListener)
  })

  onUnmounted(() => {
    document.removeEventListener('keydown', keyListener)
  })
</script>

<template>
  <div ref="root">
    <slot />
  </div>
</template>
