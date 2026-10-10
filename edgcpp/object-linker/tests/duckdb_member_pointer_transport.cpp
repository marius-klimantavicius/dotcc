struct Left { int padding; };
struct Right { int value; int increment(int amount) { value += amount; return value; } };
struct Object : Left, Right { int decrement(int amount) { value -= amount; return value; } };
using Method = int (Object::*)(int);
struct Holder { Method method; };
Method select_method(bool subtract) { return subtract ? &Object::decrement : static_cast<Method>(&Right::increment); }
Method copy_method(Method method) { Method copy = method; return copy; }
int invoke(Object &object, Method method, int amount) { return (object.*method)(amount); }
int main() {
 Object object; object.padding=71; object.value=10;
 Method method=copy_method(select_method(false));
 Holder holder{method}; Holder copied=holder;
 if(invoke(object,copied.method,7)!=17)return 1;
 method=select_method(true); holder.method=method;
 if(invoke(object,holder.method,3)!=14)return 2;
 Method null_method=nullptr;
 if(null_method || method==null_method || method!=holder.method)return 3;
 int Object::* data=static_cast<int Object::*>(&Right::value);
 return object.*data==14 && object.padding==71?0:4;
}
