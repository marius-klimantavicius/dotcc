//type: fp
//options: --c++11
// PR c++/47226
// { dg-do compile { target c++11 } }

void slurp(...) { } 
template<int ...N> 
void print() { 
  slurp([]() -> int { 
      (void) N; // or something fancy...
      return 0; 
    }() ...); 
}
