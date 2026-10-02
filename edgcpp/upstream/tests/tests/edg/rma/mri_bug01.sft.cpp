//options_all:-r -x -tused
//options: --strict;cp

template <class X>                                                             
class C {                                                                      
public:                                                                        
  void f1();                                                                   
};                                                                             
                                                                               
#pragma instantiate C<int>                                                     
                                                                               
template <class X> void C<X>::f1() {}

