enum AffiliatedKind { affiliated_first = 1, affiliated_second = 2 };

int affiliated_constant = affiliated_first;
int affiliated_select(int choose_first) {
  return choose_first ? affiliated_first : affiliated_second;
}
