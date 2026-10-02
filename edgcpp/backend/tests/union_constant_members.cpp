struct Pair { int left, right; };
union Choice {
  unsigned char first;
  unsigned long long wide;
  int* pointer;
  Pair pair;
  constexpr Choice(unsigned long long value) : wide(value) {}
  constexpr Choice(int* value) : pointer(value) {}
  constexpr Choice(Pair value) : pair(value) {}
};
static int target = 19;
static Choice wide(0x123456789abcdef0ULL);
static Choice address(&target);
static Choice nested(Pair{23, 47});
static Choice repeated[] = {Choice(11ULL), Choice(29ULL), Choice(53ULL)};
int main() {
  if (wide.wide != 0x123456789abcdef0ULL) return 1;
  if (address.pointer != &target || *address.pointer != 19) return 2;
  if (nested.pair.left != 23 || nested.pair.right != 47) return 3;
  if (repeated[0].wide != 11 || repeated[1].wide != 29 || repeated[2].wide != 53) return 4;
  *address.pointer = 61;
  wide.wide = 71;
  return target == 61 && wide.wide == 71 ? 0 : 5;
}
