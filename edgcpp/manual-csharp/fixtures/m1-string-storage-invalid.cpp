// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m1-string-storage-gate.h"
char16_t text[]=ΚΕΙΜΕΝΟ;
constexpr const char16_t* address=text;
static_assert(address[0]==233);
