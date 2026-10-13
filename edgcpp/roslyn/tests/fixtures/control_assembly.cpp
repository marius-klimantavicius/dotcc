int named_routine(int) asm("alternate_routine");

int retained_loops(int count) {
  int sum = 0;
  for (int index = 0; index < count; ++index) sum += index;
  while (sum < count) ++sum;
  do { --sum; } while (sum > count);
  return named_routine(sum);
}

asm void retained_asm_function() {
  nop
}
void use_retained_asm_function() { retained_asm_function(); }

asm("first_scope_asm");
asm("second_scope_asm");
