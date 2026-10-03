struct Day {
  unsigned char value;
  constexpr explicit Day(unsigned v) : value((unsigned char)v) {}
  constexpr operator unsigned() const { return value; }
};
constexpr unsigned last(unsigned month) {
  constexpr Day days[] = {Day(31),Day(28),Day(31),Day(30),Day(31),Day(30),
                          Day(31),Day(31),Day(30),Day(31),Day(30),Day(31)};
  return unsigned(days[month]);
}
static_assert(last(1)==28, "constant evaluation");
struct Pair { int first, second; };
static int calls;
static Pair make_pair() { ++calls; return Pair{11,19}; }
static int consume(Pair value) { return value.first+value.second; }
int main() {
  unsigned total=0;
  for(unsigned month=0;month!=12;++month) total+=last(month);
  const Pair constant=make_pair();
  Pair copy=constant;
  if(total!=365 || consume(copy)!=30 || consume(make_pair())!=30 || calls!=2) return 1;
  return 0;
}
