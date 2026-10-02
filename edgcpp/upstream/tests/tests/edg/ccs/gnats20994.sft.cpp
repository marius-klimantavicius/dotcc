//type:fn
//options::-A
//options_all:--c++17 -w

enum class A : char { } ;
enum B { };

void f()
{
  A a1{ 1 };            // Okay
  A a2 = { 1 };         // Error 1
  A a3{ 1000 };         // Error 2
  A a4 = { 1000 };      // Error 3
  A a5{ 1.5 };          // Error 4
  A a6 = { 1.5 };       // Error 5
  A a7{ 1000.5 };       // Error 6
  A a8 = { 1000.5 };    // Error 7 of 7

  B b1{1};              // Not allowed
  B b2{1.5};            // Not allowed even if narrowing
}
