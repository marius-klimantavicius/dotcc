//type:fp
//options_all:--c++20 -tused -A
   
const int i = -1;
namespace T {
     namespace N { const int i = 1; }
     namespace M {
       using namespace N;
       int a[i];
     }
   }

//cwg: 2164
//title: Name hiding and using-directives
//meeting: Jacksonville 2/18
//edg_status: Passes
