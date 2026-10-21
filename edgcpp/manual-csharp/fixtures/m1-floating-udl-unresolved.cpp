// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Lexically valid ud-suffix, semantically unresolved in this translation unit.
// Native proposal: language rejection; decoder correctly returns Unsupported
// pending actual literal-operator lookup, rather than claiming malformed syntax.
constexpr double unresolved = 1.0_scale;
