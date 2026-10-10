// Array default-member initialization uses a stock generated counted model-copy helper.
struct alignas(64) Element { int value=17; };
Element* create(unsigned long count) { return new Element[count]; }
void destroy(Element* ptr) { delete[] ptr; }
int main(){Element* empty=create(0);destroy(empty);Element* values=create(3);int result=values[0].value==17 && values[1].value==17 && values[2].value==17?0:1;destroy(values);return result;}
