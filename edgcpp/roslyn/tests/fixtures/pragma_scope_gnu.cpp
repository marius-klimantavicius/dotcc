#ident "typed-ident"
#pragma ident plain_tokens
#pragma GCC visibility push(hidden)
int pragma_hidden_variable;
#pragma GCC visibility pop
int pragma_visible_variable;

asm("");

struct PragmaNamedResult {
  PragmaNamedResult();
  PragmaNamedResult(const PragmaNamedResult&);
};

PragmaNamedResult pragma_named_result() {
  PragmaNamedResult result;
  return result;
}
