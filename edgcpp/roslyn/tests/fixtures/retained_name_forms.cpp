struct NamedDestructor { ~NamedDestructor(); };
using DestructorAlias = NamedDestructor;
void explicit_alias_destructor(NamedDestructor* object) { object->~DestructorAlias(); }

template<class T, int Number = 7> int named_overload(T) { return Number; }
template<class T, int Number = 7> int named_overload(T*) { return Number + 2; }
int explicit_template_names() {
  return named_overload<int, 9>(1) + named_overload<int>(1);
}
