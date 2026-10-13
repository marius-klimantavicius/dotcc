const unsigned char constant_embed[] = {
#embed "constant_embed.txt"
};
const unsigned char constant_embed_limited[] = {
#embed "constant_embed.txt" limit(2) prefix(7,) suffix(,11)
};
