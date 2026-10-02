//remark: [[indeterminate]]
//options:--c++26;fp

static_assert(__has_cpp_attribute(indeterminate));

int f([[indeterminate]] int);

int f([[indeterminate]] int p) {
  [[indeterminate]] int x;
  return p+x;
}

