template<class T> T&& specimen();
template<class T> T* pointer_specimen();
template<class T> auto object() -> decltype(specimen<T>().object());
template<class T> auto pointer() -> decltype(pointer_specimen<T>()->pointer());
template<class T> auto read_object(T&& value) -> decltype(object<T>().get()) { return value.object().get(); }
template<class T> auto read_pointer(T&& value) -> decltype(pointer<T>()->get()) { return value.pointer()->get(); }
struct Item { int get() { return 7; } };
struct Source { Item item; Item object() { return item; } Item* pointer() { return &item; } };
template<class T> concept Reader = requires(T value) { read_object<T>(static_cast<T&&>(value)); read_pointer<T>(static_cast<T&&>(value)); };
template<Reader T> struct Retain { T value; };
Retain<Source> source;
int main() { return read_object<Source>(static_cast<Source&&>(source.value))==7 && read_pointer<Source>(static_cast<Source&&>(source.value))==7 ? 0 : 1; }
