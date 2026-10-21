// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#define STEP 0.5f
#define STORE(left, right) left += right
#define RET(value) return value
constexpr const float& echo(const float& value) { RET(value); }
constexpr const double& echo(const double& value) { RET(value); }
template<class T> constexpr T accumulate(T start) {
  T value = start;
  int index = 0;
  while (index < 3) {
    value = echo(value) + STEP;
    index += 1;
  }
  RET(value);
}
