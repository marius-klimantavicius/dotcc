//options_all:-r -x -tused
//options: --strict;cp

void g() { }
void f() {
//  try {
//    g();
//    goto end;
//  }
//  catch(int) { goto end; }
//  catch(...) { goto end; }
//  g();                                // unreachable
//  try {
//    g();
//  }
//  catch(int) { goto end; }
//  catch(...) { goto end; }
//  g();                                // reachable
//  try {
//    g();
//    goto end;
//  }
//  catch(int) { goto end; }
//  catch(...) { }
//  g();                                // reachable
  try {
//    g();
//    goto end;
  }
//  catch(int) { }
  catch(...) { goto end; }
//  g();                                // reachable
end:;
}

