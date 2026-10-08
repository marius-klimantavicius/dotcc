#include "yyjson.hpp"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

using namespace duckdb_yyjson;

static int valid(const char *text, size_t index, bool insitu) {
    size_t length = strlen(text);
    char *input = (char *)calloc(length + YYJSON_PADDING_SIZE, 1);
    if (!input) return 1;
    memcpy(input, text, length);
    yyjson_read_err error;
    yyjson_doc *document = yyjson_read_opts(input, length, insitu ? YYJSON_READ_INSITU : 0, NULL, &error);
    if (!document || error.code || yyjson_doc_get_read_size(document) != length) return 2;
    size_t written = 0;
    char *json = yyjson_write(document, 0, &written);
    if (!json || written != strlen(json)) return 3;
    yyjson_doc *copy = yyjson_read(json, written, 0);
    if (!copy || !yyjson_equals(yyjson_doc_get_root(document), yyjson_doc_get_root(copy))) return 4;
    yyjson_mut_doc *mutable_copy = yyjson_doc_mut_copy(document, NULL);
    size_t mutable_length = 0;
    char *mutable_json = yyjson_mut_write(mutable_copy, 0, &mutable_length);
    if (!mutable_json || mutable_length != written || memcmp(json, mutable_json, written)) return 5;
    printf("valid %lu %d %s\n", (unsigned long)index, (int)insitu, json);
    free(mutable_json);
    yyjson_mut_doc_free(mutable_copy);
    yyjson_doc_free(copy);
    free(json);
    yyjson_doc_free(document);
    free(input);
    return 0;
}

int main() {
    const char *documents[] = {
        "null", "true", "false", "[]", "{}", "0", "-0.0",
        "[1,-2,3.5,1e-20,1e200,18446744073709551615,-9223372036854775808]",
        "{\"nested\":[{\"key\":\"žąsis 🦆\"},null,true,false],\"n\":42}",
        "\"a\\nb\\t\\u0000\\uD83E\\uDD86\"",
        "{\"a/b\":{\"~key\":[10,20]},\"empty\":\"\"}",
        "[\"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789\",\"0123456789012345678901234567890123456789\"]"
    };
    for (size_t i = 0; i < sizeof(documents) / sizeof(documents[0]); ++i)
        for (int insitu = 0; insitu < 2; ++insitu) {
            int status = valid(documents[i], i, insitu != 0);
            if (status) return 10 + status;
        }
    const char *invalid[] = {
        "", "[", "{\"a\":}", "[1,]", "{\"a\":1,}", "01", "1e", "tru", "null x",
        "\"\\uD800\"", "\"\\uDD00\"", "\"\\q\"", "\"a\nb\"", "\"\xc0\xaf\"", "NaN", "Infinity"
    };
    for (size_t i = 0; i < sizeof(invalid) / sizeof(invalid[0]); ++i) {
        yyjson_read_err error;
        yyjson_doc *document = yyjson_read_opts((char *)invalid[i], strlen(invalid[i]), 0, NULL, &error);
        if (document || !error.code) return 20;
        printf("invalid %lu %u %lu\n", (unsigned long)i, (unsigned)error.code, (unsigned long)error.pos);
    }
    const char *extension = "/*comment*/[1,2,]";
    yyjson_doc *extended = yyjson_read(extension, strlen(extension), YYJSON_READ_ALLOW_COMMENTS | YYJSON_READ_ALLOW_TRAILING_COMMAS);
    if (!extended || yyjson_arr_size(yyjson_doc_get_root(extended)) != 2) return 21;
    yyjson_doc_free(extended);
    yyjson_mut_doc *document = yyjson_mut_doc_new(NULL);
    if (!document) return 22;
    yyjson_mut_val *root = yyjson_mut_obj(document);
    yyjson_mut_doc_set_root(document, root);
    if (!yyjson_mut_obj_add_str(document, root, "text", "žąsis 🦆") ||
        !yyjson_mut_obj_add_uint(document, root, "largest", UINT64_MAX) ||
        !yyjson_mut_obj_add_null(document, root, "missing")) return 23;
    yyjson_mut_val *array = yyjson_mut_arr(document);
    if (!yyjson_mut_obj_add_val(document, root, "values", array)) return 24;
    for (int i = 0; i < 1024; ++i) if (!yyjson_mut_arr_add_sint(document, array, i - 512)) return 25;
    size_t length;
    char *text = yyjson_mut_write(document, 0, &length);
    if (!text) return 26;
    yyjson_doc *readback = yyjson_read(text, length, 0);
    yyjson_val *read_root = yyjson_doc_get_root(readback);
    if (!readback || !yyjson_equals_str(yyjson_obj_get(read_root, "text"), "žąsis 🦆") ||
        yyjson_get_uint(yyjson_obj_get(read_root, "largest")) != UINT64_MAX ||
        yyjson_arr_size(yyjson_obj_get(read_root, "values")) != 1024) return 27;
    printf("mutated %s\n", text);
    free(text);
    yyjson_doc_free(readback);
    yyjson_mut_doc_free(document);
    puts("extensions and mutation passed");
    return 0;
}
