//options_all:--c++23 -tused -A
   int h1(int (&)[]);
   int h1(int (&)[1]);
   int h2(void (&)());
   int h2(void (&)() noexcept);
   void g2() {
     int a[1];
     h1(a);            // calls h1(int (&)[1])
     extern void f2() noexcept;
     h2(f2);            // calls h2(void (&)() noexcept)
   }
