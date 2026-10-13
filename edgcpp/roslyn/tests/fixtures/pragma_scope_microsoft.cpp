#pragma comment(lib, "native_semantic.lib")
#pragma conform(forScope, push, semantic_marker, on)
int pragma_microsoft_control;
#pragma conform(forScope, pop, semantic_marker)
#pragma include_alias("semantic_long.hpp", "semantic_short.hpp")

template<class T> struct PragmaDependentExists {
  __if_exists(T::member) { int present; }
  __if_not_exists(T::member) { int absent; }
};
