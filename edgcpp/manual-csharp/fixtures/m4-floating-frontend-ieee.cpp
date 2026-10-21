// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr double quotient(double left, double right) { return left / right; }
constexpr double infinity = quotient(1.0, 0.0);
constexpr double not_number = quotient(0.0, 0.0);
constexpr double invalid = infinity - infinity;
constexpr float narrow_infinity = infinity;
constexpr bool truth = not_number;
constexpr double transport() {
  double cells[2];
  cells[0] = infinity;
  cells[1] = not_number;
  const double& alias = cells[0];
  return alias + cells[1];
}
constexpr double carried = transport();
static_assert(infinity > 1.0);
static_assert(not_number != not_number);
static_assert(!(not_number == not_number));
static_assert(!(not_number < 0.0) && !(not_number >= 0.0));
static_assert(invalid != invalid);
static_assert(narrow_infinity == infinity);
static_assert(truth);
static_assert(carried != carried);
