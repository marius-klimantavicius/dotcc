  template<typename> struct B;
  template<typename T> struct D: B<T> { 
    using B<T>::B;  // Dependent inheriting constructor.
  };

