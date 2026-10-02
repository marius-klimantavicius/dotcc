//options_all:-r -x -tused
//options: --strict;cp

void f() {
  auto     union { int auto_anon_Umember; };
  static   union { int static_anon_Umember; };
  register union { int register_anon_Umember; };
}


