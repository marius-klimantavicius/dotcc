//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

constexpr int main();  // { dg-error "1:cannot declare .::main. to be .constexpr." }
