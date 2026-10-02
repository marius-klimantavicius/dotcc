// remark: Covariant returns and accessibility
// options:;fp:-DNEG;fn

   class BR {};
   class DR : BR {
     friend class C1;
     friend class C2;
   };
   class B { virtual BR *f(); };
   class C1 : B { DR *f(); };
   class D1 : C1 { DR *f(); };
   class C2 : B { BR *f(); };
#ifdef NEG
   class D2 : C2 { DR *f(); };
#endif
