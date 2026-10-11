namespace Recipes {
template<class T, int Value> constexpr int choose() { return Value; }
}

template<class T, int Value = Recipes::choose<T, 3>()>
int selected() { return Value; }

int main() {
    return selected<int>() == 3 && selected<long, 9>() == 9 ? 0 : 1;
}
