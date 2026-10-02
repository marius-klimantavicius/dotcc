//type:fp
//options_all:--c++17 -tused -A
  template <class T1, class T2, int I> struct B {
    B<T1, T2, I>* b1;        // refers to the current instantiation
    B<T2, T1, I>* b2;        // not the current instantiation
    typedef T1 my_T1;
    static const int my_I = I;
    static const int my_I2 = I+0;
    static const int my_I3 = my_I;
    static const long my_I4 = I;
    static const int my_I5 = (I);
    B<my_T1, T2, my_I>* b3;   // refers to the current instantiation
    B<my_T1, T2, my_I2>* b4;  // not the current instantiation
    B<my_T1, T2, my_I3>* b5;  // refers to the current instantiation
    B<my_T1, T2, my_I4>* b6;  // not the current instantiation
    B<my_T1, T2, my_I5>* b7;  // not the current instantiation
  };

//cwg: 2307
//title: Unclear definition of “equivalent to a nontype template parameter”
//meeting: Albuquerque 11/17
//edg_status: Passes
