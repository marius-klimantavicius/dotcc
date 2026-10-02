//options_all:-r -x -tused
//options: --strict;cp

void g() { }
void f() {
  try {
    g();
  }
  catch (int) { }
  catch (char) { }
}

