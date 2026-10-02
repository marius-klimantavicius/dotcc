//remark:Binding rvalue ref to char array
//options:--c++20;fp

int f(const char (&&)[4]); // #1
int f(const char (&&)[5]); // #2
int r = f({"abc"});
