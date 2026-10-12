#include "shared.h"

int main() {
  if (provider_digits() != digits || provider_digits()[2] != '2') return 1;
  if (provider_count() != 18 || strong_value != 29) return 2;
  left::counter = 13;
  return provider_count() != 24;
}
