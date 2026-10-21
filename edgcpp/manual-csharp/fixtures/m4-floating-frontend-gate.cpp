// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m4-floating-frontend-gate.h"
void echo(float& value);
constexpr float work(float start) {
  float cells[2];
  cells[0] = accumulate(start);
  cells[1] = 1.0f;
  float& alias = cells[0];
  while (cells[1] < 3.0f) {
    STORE(alias, STEP);
    cells[1] += 1.0f;
  }
  RET(alias);
}
constexpr float run() {
  int echo = 0;
  return work(1.0f);
}
static_assert(run() == 3.5f);
static_assert(accumulate(1.0) == 2.5);
static_assert(accumulate(2.0f) == 3.5f);
static_assert(accumulate(1.0f) == 2.5f);
static_assert(sizeof(accumulate(1.0f)) == 4);
static_assert(sizeof(accumulate(1.0)) == 8);
