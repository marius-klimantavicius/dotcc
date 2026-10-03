// Named locals must retain native layout, stable addresses, and EH cleanup.
struct Pair { int left; double right; };
struct __attribute__((packed)) Packed { char tag; long long value; };
union Choice { long long integer; double real; };
struct Base {
  int number;
  char marker;
  Base() : number(13), marker(7) {}
};
struct Derived : Base { char tail; Derived() : tail(9) {} };

int destruction_log;
struct Guard {
  int value;
  Guard(int n) : value(n) {}
  ~Guard() { destruction_log = destruction_log * 10 + value; }
};
void raise_error() { throw 19; }
int ordinary_storage(int selected) {
  Pair item = {3, 4.5};
  Pair items[2] = {{5, 6.5}, {7, 8.5}};
  Pair *links[2] = {&item, &items[1]};
  Choice choice;
  choice.integer = 123;
  Packed packed;
  packed.tag = 2;
  packed.value = 987654321;
  if (((unsigned long long)&item & 7) ||
      ((unsigned long long)&items & 7) ||
      ((unsigned long long)&links & 7) ||
      ((unsigned long long)&choice & 7)) return 1;
  if (sizeof(Pair) != 16 || sizeof(Packed) != 9 ||
      ((char*)&packed.value - (char*)&packed) != 1) return 2;
  if (!selected) goto done;
  links[0]->left += links[1]->left;
  packed.value += choice.integer;
 done:
  if (item.left != (selected ? 10 : 3) || packed.value != (selected ? 987654444 : 987654321)) return 3;
  if (items[0].right != 6.5) return 4;
  return 0;
}
int base_storage() {
  Derived derived;
  return derived.number != 13 || derived.marker != 7 || derived.tail != 9;
}
void unwind_named_locals() {
  Guard first(1);
  Guard second(2);
  raise_error();
}
int aligned_storage() {
  alignas(64) Pair item = {17, 19.5};
  if ((unsigned long long)&item & 63) return 1;
  alignas(32) Pair *pointer = &item;
  if ((unsigned long long)&pointer & 31) return 2;
  return pointer->left != 17 || pointer->right != 19.5;
}
int large_storage() {
  char data[70000];
  data[0] = 11;
  data[69999] = 23;
  return data[0] != 11 || data[69999] != 23;
}
int main() {
  if (ordinary_storage(0) || ordinary_storage(1)) return 1;
  if (aligned_storage() || base_storage()) return 2;
  if (large_storage()) return 3;
  try { unwind_named_locals(); } catch (int error) {
    if (error != 19 || destruction_log != 21) return 4;
    return 0;
  }
  return 5;
}
