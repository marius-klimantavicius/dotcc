<!--
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
-->

<script setup>
  import { computed, ref, onUnmounted } from 'vue'

  import { acquireSocketForReviews } from './sockets.js'

  import LoadingSpinner from './LoadingSpinner.vue'

  // Open a socket to discover reviews
  const socket = acquireSocketForReviews()

  const socketOpen = ref(true)
  const reviews = ref({})

  const defaultTabForReview = (review) => {
    if (review.sockets.test_source) {
      return 'review-tests'
    }
    if (review.sockets.bench_source) {
      return 'review-benchmarks'
    }
    return 'review-tests'
  }

  const reviewIsOpenable = (review) => {
    return review.sockets.test_source || review.sockets.bench_source
  }

  const describeReviewtype = (review) => {
    if (review.sockets.test_source && review.sockets.bench_source) {
      return "Test & Benchmark"
    } else if (review.sockets.test_source) {
      return "Test"
    } else if (review.sockets.bench_source) {
      return "Benchmark"
    }
    return "Oprhan"
  }

  const formatCreatedAt = (createdAt) => {
    if (!createdAt) {
      return ''
    }
    const date = new Date(createdAt)
    if (Number.isNaN(date.getTime())) {
      return createdAt
    }
    return date.toLocaleString()
  }

  const formatReviewName = (review) => {
    const started = formatCreatedAt(review.created_at)
    if (!started) {
      return review.name
    }
    return `${review.name} - ${started}`
  }

  const sortedReviews = computed(() => {
    return Object.values(reviews.value).sort((a, b) => {
      const aTime = Date.parse(a.created_at) || 0
      const bTime = Date.parse(b.created_at) || 0
      return bTime - aTime
    })
  })

  const reviewsFromList = (list) => {
    const mapped = {}
    for (const review of list) {
      mapped[String(review.id)] = review
    }
    return mapped
  }

  const applyReviewListPatch = (patch) => {
    if (patch.remove) {
      for (const id of patch.remove) {
        delete reviews.value[String(id)]
      }
    }

    if (patch.upsert) {
      for (const review of patch.upsert) {
        reviews.value[String(review.id)] = review
      }
    }
  }

  const onSocketMessage = (event) => {
    let message = JSON.parse(event.data)

    if (message.t === "list-reviews") {
      reviews.value = reviewsFromList(message.v)
    } else if (message.t === "list-reviews-patch") {
      applyReviewListPatch(message.v)
    } else {
      console.assert(false, `Unexpected reviews list message: ${message.t}`)
    }
  }

  const onSocketClose = () => {
    socketOpen.value = false
  }

  socket.addEventListener("message", onSocketMessage)
  socket.addEventListener("close", onSocketClose)

  onUnmounted(() => {
    socket.removeEventListener("message", onSocketMessage)
    socket.removeEventListener("close", onSocketClose)
    socket.close()
  })
</script>

<template>
  <div class="cell cell-colors">
    <div class="heading cell-colors">
      <h1>
        AcknowlEDG
        <loading-spinner
            v-if="socketOpen"
            style="position: absolute; right: 0px;"/>
      </h1>
    </div>
    <div v-if="socketOpen">
      <table style="width: 100%;">
        <colgroup>
          <col span="1">
          <col span="2" width="0*">
        </colgroup>
        <thead>
          <tr>
            <th style="text-align: left; padding: 0px;">Name</th>
            <th>Review Connections</th>
            <th>Review Type</th>
          </tr>
        </thead>
        <transition-group name="review-list" tag="tbody" appear>
          <tr v-for="review in sortedReviews" :key="review.id">
            <td>
              <router-link v-if="reviewIsOpenable(review)"
                           :to="{ name: defaultTabForReview(review),
                                  params: { id: review.id, slug: '_' } }">
                {{ formatReviewName(review) }}
              </router-link>
              <span v-if="!reviewIsOpenable(review)">
                {{ formatReviewName(review) }}
              </span>
            </td>
            <td style="text-align: center;">
              {{ review.sockets.reviewer }}
            </td>
            <td style="text-align: center;">
              {{ describeReviewtype(review) }}
            </td>
          </tr>
        </transition-group>
      </table>
    </div>
    <div v-if="!socketOpen">
      Connection to server could not be established;
      please reload the page and try again.
    </div>
  </div>
</template>

<style scoped>
  .cell {
    padding-left: 5px;
    padding-right: 5px;
    border-radius: 8px;
    margin: 10px;
    padding: 20px;
    height: calc(100vh - 60px);
    overflow: auto;
  }
  @media (prefers-color-scheme: light) {
    .cell-colors {
      color: black;
      background-color: white;
    }
  }
  @media (prefers-color-scheme: dark) {
    .cell-colors {
      color: white;
      background-color: #181A1B;
    }
    a {
      color: skyblue;
    }
  }
  .heading {
    position: sticky;
    top: 0px;
  }
  h1 {
   margin-top: 0px;
  }
  th {
    padding-left: 1em;
    padding-right: 1em;
    white-space: nowrap;
  }
  .review-list-move {
    transition: transform 0.45s ease;
  }
  .review-list-enter-active,
  .review-list-leave-active {
    transition: opacity 0.35s ease, transform 0.35s ease;
  }
  .review-list-enter-from {
    opacity: 0;
    transform: translateX(-12px);
  }
  .review-list-leave-to {
    opacity: 0;
    transform: translateX(24px);
  }
  @media (prefers-reduced-motion: reduce) {
    .review-list-move,
    .review-list-enter-active,
    .review-list-leave-active {
      transition: none;
    }
  }
</style>
