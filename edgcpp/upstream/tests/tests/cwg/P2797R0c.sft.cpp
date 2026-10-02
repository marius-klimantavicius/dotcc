//type:fn
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
     c();               // error: ambiguous between #2 and #3
     (C::c)();          // error: as above
     (&(C::c))();       // error: cannot resolve address of overloaded this->C::c [over.over]
     (&C::c)(C{});      // selects #1
     (&C::c)(*this);    // error: selects #2, and is ill-formed [over.match.call.general]/2
     (&C::c)();         // selects #3
   }
};
