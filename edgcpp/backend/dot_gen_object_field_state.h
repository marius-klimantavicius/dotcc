#ifndef DOT_GEN_OBJECT_FIELD_STATE_H
#define DOT_GEN_OBJECT_FIELD_STATE_H
// Availability observations only: never initialize or change an EDG field.
// The pinned clear_variable does not initialize a_variable::uninitialized;
// def_initializer contains its sole assignment. Reading it before an observed
// assignment would serialize indeterminate storage as a semantic result.
#include <cstdlib>
#include <cstring>
#include <set>
BEGIN_EDG_NAMESPACE
namespace dot_object_source { namespace field_observation {
inline bool enabled() {
  static const bool value=[](){auto setting=std::getenv("EDG_DOT_OBJECT");return setting && std::strcmp(setting,"1")==0;}();
  return value;
}
inline std::set<const void*> &assigned_variables() {
  static std::set<const void*> value;return value;
}
inline void variable_cleared(const void *pointer) {
  if(enabled())assigned_variables().erase(pointer);
}
inline void variable_uninitialized_assigned(const void *pointer) {
  if(enabled())assigned_variables().insert(pointer);
}
inline bool variable_uninitialized_available(const void *pointer) {
  return assigned_variables().count(pointer)!=0;
}
inline void variable_copied(const void *source,const void *destination) {
  if(!enabled())return;
  if(variable_uninitialized_available(source))assigned_variables().insert(destination);
  else assigned_variables().erase(destination);
}
} }
END_EDG_NAMESPACE
#endif
