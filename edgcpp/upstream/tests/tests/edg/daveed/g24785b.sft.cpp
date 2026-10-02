//remark:C++23 multi-subscript
//options:--c++23;fp:--c++23 -DNEG;fn


struct S {
  int operator[](int, int);
} s;
int r = s[1, 2];

int arr[10];

#ifdef NEG
int e = arr[1, 2];
#else
int i = arr[2];
#endif

