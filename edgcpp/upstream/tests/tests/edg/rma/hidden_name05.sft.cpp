//options_all:-r -x -tused
//options: --strict;cp

struct A { 
  void open(); 
}; 
extern int open(); 
void A::open() { 
  int i = ::open();
} 


