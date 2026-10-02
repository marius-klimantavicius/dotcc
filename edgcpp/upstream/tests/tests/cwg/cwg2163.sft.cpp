//type:fn
//options_all:--c++17 -tused -A
//
constexpr int foo1(int n, bool runtime_only = false) {
   int i = n + 10;
   if (runtime_only) {
     volatile int V = 0;
     [&] { i += V; }();
   }
   return i;
UnReferencedLabel:
}

//cwg: 2163
//title: Labels in constexpr functions
//meeting: Jacksonville 2/16
//edg_status: Passes
