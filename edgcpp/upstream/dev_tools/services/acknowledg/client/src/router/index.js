/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

import { createRouter, createWebHistory } from 'vue-router'

import ReviewsView from '../ReviewsView.vue'
import ReviewShell from '../ReviewShell.vue'
import TestReviewView from '../TestReviewView.vue'
import BenchReviewView from '../BenchReviewView.vue'

const routes = [
  {
    path: '/',
    name: 'reviews',
    component: ReviewsView
  },
  {
    path: '/review/:id',
    component: ReviewShell,
    children: [
      {
        path: '',
        redirect: (to) => ({
          name: 'review-tests',
          params: { id: to.params.id, slug: '_' },
          hash: to.hash
        })
      },
      {
        path: 'tests',
        redirect: (to) => ({
          name: 'review-tests',
          params: { id: to.params.id, slug: '_' },
          hash: to.hash
        })
      },
      {
        path: 'tests/:slug',
        name: 'review-tests',
        component: TestReviewView
      },
      {
        path: 'benchmarks',
        redirect: (to) => ({
          name: 'review-benchmarks',
          params: { id: to.params.id, slug: '_' },
          hash: to.hash
        })
      },
      {
        path: 'benchmarks/:slug',
        name: 'review-benchmarks',
        component: BenchReviewView
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routes,
})

export default router
