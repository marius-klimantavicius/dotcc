//type: fp
//options:  --c++20 --modules
// PR c++/114229
// { dg-module-do link }
// { dg-additional-options "-fmodules-ts -fno-module-lazy" }

import modA;
import modB;
int main() { }
