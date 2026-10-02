//remark:if consteval
//options:--c++23;fn

int f() {
  goto X;
  if not consteval {
X:
    return 3;
goto Y;
  } else {
Y:
    return 4;
  }
}

int g(int i) {
switch (i) {
  if not consteval {
case 1:
    return 3;
  } else {
case 2:
    return 4;
  }
default:;
}
}
