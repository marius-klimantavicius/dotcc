//type:fn
//options_all_--c++27 -tused -A
//Converting from T* to bool should be considered narrowing. (P1957R2)
bool b = {"meow"}; // error: narrows
