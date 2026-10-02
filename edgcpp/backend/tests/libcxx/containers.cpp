#include <vector>
#include <deque>
#include <list>
#include <forward_list>
#include <map>
#include <set>
#include <unordered_map>
#include <unordered_set>
#include <string>

int main() {
  std::vector<int> vector{1, 2, 3};
  for (int i = 4; i <= 100; ++i) vector.push_back(i);
  vector.erase(vector.begin() + 1, vector.begin() + 9);
  if (vector.size() != 92 || vector[1] != 10 || vector.back() != 100) return 1;
  auto copy = vector;
  vector[0] = -1;
  if (copy[0] != 1) return 2;
  std::deque<int> deque;
  for (int i = 0; i < 1100; ++i) { deque.push_back(i); deque.push_front(-i); }
  for (int i = 0; i < 300; ++i) { deque.pop_front(); deque.pop_back(); }
  if (deque.front() != -799 || deque.back() != 799 || deque.size() != 1600) return 3;
  std::list<int> left{3, 1}, right{4, 2};
  auto stable = right.begin();
  left.splice(left.end(), right);
  left.sort();
  if (!right.empty() || *stable != 4 || left.front() != 1 || left.back() != 4) return 4;
  std::forward_list<int> forward{1, 2, 2, 3};
  forward.unique(); forward.reverse();
  if (forward.front() != 3) return 5;
  std::map<std::string, int> tree{{"two", 2}, {"one", 1}};
  tree.try_emplace("three", 3);
  tree.insert_or_assign("two", 22);
  if (tree.at("two") != 22 || tree.begin()->first != "one" || tree.size() != 3) return 6;
  auto node = tree.extract("two"); node.key() = "four"; tree.insert(std::move(node));
  if (tree.count("two") || tree.at("four") != 22) return 7;
  std::set<int> set{7, 3, 7, 5};
  if (set.size() != 3 || *set.lower_bound(4) != 5) return 8;
  std::unordered_map<int, std::string> hash;
  for (int i = 0; i < 200; ++i) hash.emplace(i, std::string(30, static_cast<char>('a' + i % 26)));
  hash.reserve(700);
  if (hash.at(123) != std::string(30, 't')) return 9;
  std::unordered_set<int> hashed_set{1, 2, 1, 3};
  return hashed_set.size() == 3 && hashed_set.contains(2) ? 0 : 10;
}
