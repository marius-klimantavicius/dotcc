struct ConstantCValues { int first; int second; int tail[4]; };
union ConstantCUnion { int integer; double real; };
struct ConstantCValues constant_designated = {.second = 7, .tail[2] = 11};
union ConstantCUnion constant_union = {.real = 2.5};
int constant_repeated[8] = {[2 ... 5] = 13};
int constant_repeat_tail[3][4] = {[0 ... 2] = {[1] = 17, 19}};
struct ConstantCValues *constant_compound_address =
    &(struct ConstantCValues){.first = 23, .tail = {29}};
int constant_stmt_folded(void) {
  return ({ const int value = 31; value + 1; });
}
struct ConstantFlexible { int count; char bytes[]; };
struct ConstantFlexible constant_flexible = {3, "abc"};
