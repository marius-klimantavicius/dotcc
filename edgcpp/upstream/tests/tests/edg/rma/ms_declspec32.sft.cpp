//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

void foo() {
  _declspec(dllimport) int x = 0;  // error
}

