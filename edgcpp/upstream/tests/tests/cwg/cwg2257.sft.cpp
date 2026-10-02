//type: rp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
extern "C" int printf(const char* format, ...);

  struct A { 
    bool live; 
    A() : live(true) { 
      static int cnt; 
      if (cnt++ == 1) throw 0; 
    } 
    ~A() { 
      printf("live: %d\n", live); 
      live = false; 
    } 
  }; 
  struct AA { A &&a0, &&a1; }; 
  void doit() { 
    static AA aa = { A(), A() }; 
  } 
  int main(void) { 
    try { 
      doit(); 
    } 
    catch (...) { 
      printf("in catch\n"); 
      doit(); 
    } 
  }

//cwg: 2257
//title: Lifetime extension of references vs exceptions
//meeting: Kona 02/19
//edg_status: Passes
