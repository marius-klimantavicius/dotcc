//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

class __declspec(dllimport) A { };
class __declspec(dllexport) A;
class __declspec(dllimport) B;
class __declspec(dllexport) B { };
main() {
  A a;
  B b;
}

