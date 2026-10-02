//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap
// we give a false positive
// 
   class V {
   public:
     virtual ~V() noexcept(false);
   };

   class B : virtual V {
     virtual void foo () = 0;
     // implicitly defined virtual ~B () noexcept(true);
   };

   class D : B {
     virtual void foo ();
     // implicitly defined virtual ~D () noexcept(false);
   };

  static_assert(!noexcept(D()));

//cwg: 2336
//title: Destructor characteristics vs potentially-constructed subobjects
//meeting: Kona 02/19
//edg_status: Passes
