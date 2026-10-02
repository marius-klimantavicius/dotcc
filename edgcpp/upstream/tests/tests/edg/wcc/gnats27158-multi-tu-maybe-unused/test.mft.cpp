//type:fp
//options:--c++17:--c++11 --g++:--c++03 --g++
//options_all:--multi_trans_unit other-tu.c
#include "shared.h"

#if TEST_NUMBER == 1
void foo([[maybe_unused]] int x) {
#elif TEST_NUMBER == 2
void foo([[gnu::unused]] int x) {
#elif TEST_NUMBER == 3
void foo(__attribute__((unused)) int x) {
#endif
}
