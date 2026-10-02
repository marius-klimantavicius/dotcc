//type:fp
//options:--c++20 --no_il_lower --il_display
//filter:awk '/^file-scope (requires-clause|routine)@/{f=1}/^$/{if (f) print $0; f=0}f' | grep -E -e '^(  name|  decl_position[.]seq|constraint|requires_pos[.]seq|type|is_prototype_instantiation|is_template_function|is_specialized|defined|trailing_requires_clause|assoc_template):' -e '^file-scope ' -e '^$' | sed -e 's/@[0-9a-f]*//'

template<typename> concept X = true;

template<typename T> struct C
{
  template<typename U>
  void f(T, U) requires X<T>;

  void g(T, int) requires X<T>;
};

template<typename U>
void h(U) requires X<U>;

template<>
void h(int);

template<>
void h(long)
{ }

template<> template<typename U>
void C<int>::f(int, U) requires X<int>;

template<> template<typename U>
void C<long>::f(long, U) requires X<long>
{ }

template<>
void C<int>::g(int, int);

template<>
void C<long>::g(long, int)
{ }
