//type:fp
//options:--gn 160100

void f()
{
  __builtin_is_string_literal("");
  __builtin_constexpr_diag(1);
}
