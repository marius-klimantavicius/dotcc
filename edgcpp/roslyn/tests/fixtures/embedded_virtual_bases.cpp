struct EmbeddedRoot { int value; };
struct EmbeddedMiddle : virtual EmbeddedRoot { int middle; };
struct EmbeddedLeaf : virtual EmbeddedMiddle { int leaf; };
EmbeddedLeaf embedded_leaf;
