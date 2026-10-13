struct RoutineException { int value; };
void routine_throw_list() throw(int, RoutineException) { }
void routine_throw_empty() throw() { }
void routine_noexcept_true() noexcept(true) { }
void routine_noexcept_false() noexcept(false) { }

template<bool Safe> int routine_noexcept_dependent() noexcept(Safe) { return Safe; }
template<class T> struct RoutineExceptionMember {
  T value;
  RoutineExceptionMember() = default;
  ~RoutineExceptionMember() = default;
};
RoutineExceptionMember<RoutineException> routine_exception_member;
template<class T> struct RoutineExceptionDeferred {
  void pending() noexcept(sizeof(T) > 0);
  template<class U> void subordinate() noexcept(sizeof(T) + sizeof(U) > 0);
};
RoutineExceptionDeferred<int> routine_exception_deferred;
int routine_exception_use() {
  routine_throw_list();
  routine_throw_empty();
  routine_noexcept_true();
  routine_noexcept_false();
  return routine_noexcept_dependent<true>() + routine_noexcept_dependent<false>();
}
