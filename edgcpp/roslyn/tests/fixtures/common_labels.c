int common_local_labels(int value) {
  __label__ first, second;
  void *destination = value ? &&first : &&second;
  goto *destination;
first:
  return 3;
second:
  return 5;
}
