#pragma test_bind_next_pass declaration
int bound_declaration;
int bound_statement(int input) {
#pragma test_bind_next_pass statement
  return input + bound_declaration;
}

namespace RetainedNamespace { int value = 5; }
int retained_local_alias() {
  namespace LocalAlias = RetainedNamespace;
  return LocalAlias::value;
}
