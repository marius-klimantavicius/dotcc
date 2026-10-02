#include <vector>
int main() {
    std::vector<int> values{20, 22};
    return values[0] + values[1] == 42 ? 0 : 1;
}
