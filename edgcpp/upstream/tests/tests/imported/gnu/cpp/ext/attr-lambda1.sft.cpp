//type: fp
//options: --c++11
// PR c++/89640
// { dg-options "" }
// { dg-do compile { target c++11 } }

void test() {
    []() __attribute__((noinline,cold)) {
        asm volatile("");
    }();
}
