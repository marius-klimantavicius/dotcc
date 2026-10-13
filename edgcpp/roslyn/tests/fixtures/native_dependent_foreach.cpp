template<class T> void native_dependent_foreach(T &collection) {
  for each(auto value in collection) {
    (void)value;
  }
}
