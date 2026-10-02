#include "shared.h"

static int private_value = 11;

int main() {
  if (initialized != 9 || private_value != 11) return 1;
  if (address_other() != &counter() || counter() != 3) return 2;
  counter() = 7;
  if (call_other() != 28 || twice(2) != 4) return 3;
  try {
    throw_other();
  } catch (Base& value) {
    if (value.get() != 13 || dynamic_cast<Derived*>(&value) == 0) return 4;
    return 0;
  }
  return 5;
}
