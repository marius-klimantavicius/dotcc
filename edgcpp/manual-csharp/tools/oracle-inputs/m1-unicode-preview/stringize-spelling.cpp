// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#define S(x) #x
S(\u00E9)
S(é)
S(\U00010400)
S("\u00E9")
S(U'\U00010400')
