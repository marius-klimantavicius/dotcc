template<class T> struct Argument { T value; };
void consume(Argument<long>);
auto get_callback(){return &consume;}
