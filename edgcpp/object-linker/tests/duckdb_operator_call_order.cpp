enum Flags { First = 1, Second = 2 };
int trace = 0;
Flags value = First;
Flags &left() { trace = trace * 10 + 1; return value; }
Flags right() { trace = trace * 10 + 2; return Second; }
Flags &operator|=(Flags &target, Flags source) {
  target = Flags(int(target) | int(source)); return target;
}
int main() { left() |= right(); return trace == 21 && int(value) == 3 ? 0 : 1; }
