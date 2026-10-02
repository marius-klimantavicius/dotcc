//type:fn
//options_all:--c++20 -tused -A
void h();            // #2
namespace h {}       // error: same entity as #2, but not a function
