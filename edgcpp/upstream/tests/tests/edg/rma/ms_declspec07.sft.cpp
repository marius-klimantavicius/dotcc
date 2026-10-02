//options_all:-r -x -tused
//options: --microsoft -n;cp

__declspec(selectany) int x1 = 1;
//const __declspec(selectany) int x2=2;    // error
extern const __declspec(selectany) int x3=3;
extern const int x4;
const __declspec(selectany) int x4=4;
//extern __declspec(selectany) int x5;     // error

