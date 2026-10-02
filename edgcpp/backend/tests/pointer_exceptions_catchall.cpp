// No typed handler may incidentally supply these thrown types' RTTI roots.
struct S { int value; };
enum E { x };
int count;
struct Guard { ~Guard() { ++count; } };
int main() {
  S s={1}; E e=x; const S* p=&s; const S** pp=&p;
  try { Guard g; throw &s; } catch (...) {}
  try { Guard g; throw &e; } catch (...) {}
  try { Guard g; throw pp; } catch (...) {}
  try { Guard g; throw nullptr; } catch (...) {}
  return count==4?0:1;
}
