//remark:Address of _Generic lvalue
//options:--c11 --gnu=100500;fp

  struct S* arr[1];
  struct S** p = _Generic(arr, struct S**: arr);  // Previously an error in
                                                  // some configurations.

