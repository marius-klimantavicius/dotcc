//options_all:--c++23 -tused -A
  struct C {
    void a();
    void b() {
      a();                // OK, (*this).a()
    }

   void c(this const C&);   // #1
   void c()&;               // #2
   static void c(int = 0);  // #3

   void d() {
     (&C::c)(C{});      // selects #1
     (&C::c)();         // selects #3
   }
};
