//remark:Binding rvalue ref to char array
//options:--c++20;fn:-DALL --c++20;fn

#ifdef ALL
const char (&&r4)[4] = {"abc"};
const char (&&r5)[5] = {"abc"};
#endif
const char (&&r_)[] = {"abc"};
