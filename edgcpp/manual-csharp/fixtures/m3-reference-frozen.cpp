// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Used instantiation must retain the earlier int-returning reference overload.
int helper(const int&);
template<class T> int frozen(T x){return helper(x);}
void helper(int&);
int run(){return frozen(1);}
