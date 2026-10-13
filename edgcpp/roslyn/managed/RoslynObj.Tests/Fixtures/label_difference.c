int label_difference(int index) {
  static const long offsets[] = { &&first - &&base, &&second - &&base };
  if (index < 0 || index > 1) return -1;
  goto *((char *)&&base + offsets[index]);
base:
  return 0;
first:
  return 1;
second:
  return 2;
}

int main(void) { return label_difference(1); }
