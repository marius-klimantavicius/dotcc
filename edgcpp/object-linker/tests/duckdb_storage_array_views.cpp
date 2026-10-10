template<class T> struct Data {
    static const unsigned long values[];
    static const short exponents[];
    static const int grid[][2];
};
template<class T> const unsigned long Data<T>::values[] = {10, 20, 30};
template<class T> const short Data<T>::exponents[] = {1, 2, 3};
template<class T> const int Data<T>::grid[][2] = {{4, 5}, {6, 7}};
extern template struct Data<void>;
int read(int index) { return int(Data<void>::values[index]) + Data<void>::exponents[index]; }
int main() { return read(2) == 33 && Data<void>::grid[1][1] == 7 ? 0 : 1; }

template struct Data<void>;
