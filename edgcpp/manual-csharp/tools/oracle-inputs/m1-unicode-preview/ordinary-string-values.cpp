// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
static_assert(sizeof("\u00E9")==3);
static_assert(sizeof("é")==3);
constexpr const char* direct="é";
static_assert(static_cast<unsigned char>(direct[0])==0xc3 && static_cast<unsigned char>(direct[1])==0xa9);
