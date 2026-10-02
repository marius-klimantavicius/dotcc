//type:fn
//options_all:-tused --c++20
void f(){ 
    int x=0; 
    auto g=[x](int x){return 0;};		//error: parameter and capture have the same name 
    auto h=[y=0]<typename y>(y){return 0;};	//error: parameter and capture have the same name
}
