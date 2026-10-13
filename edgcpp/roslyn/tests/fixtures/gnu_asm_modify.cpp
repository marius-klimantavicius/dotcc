int gnu_asm_modify(int value) {
  asm volatile ("" : "+r" (value));
  return value;
}
