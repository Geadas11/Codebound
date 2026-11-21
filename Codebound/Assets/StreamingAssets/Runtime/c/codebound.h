/* Codebound — game API for C and C++ programs (compiled to WebAssembly, run in the game's sandbox).
 *
 *   #include <codebound.h>
 *   int main(void) {
 *       char drill[32], item[64];
 *       if (!cb_find("drill", drill, sizeof drill)) { printf("no drill nearby\n"); return 0; }
 *       while (cb_op(drill, "mine", "[]", item, sizeof item) > 0) printf("mined %s\n", item);
 *   }
 *
 * Results are JSON text (the same values JavaScript/Python get). cb_num() / cb_str() read simple fields.
 * printf() and puts() write to the computer's log. sleep()/cb_sleep() wait in game time.
 */
#ifndef CODEBOUND_H
#define CODEBOUND_H
#include <stddef.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

#ifdef __cplusplus
extern "C" {
#endif

#define CB_IMPORT(name) __attribute__((import_module("codebound"), import_name(name)))

/* raw host calls (provided by the game) */
CB_IMPORT("call") int cb__call(const char *obj, int obj_len, const char *fn, int fn_len, const char *args, int args_len, char *out, int cap);
CB_IMPORT("error") int cb__error(char *out, int cap);
CB_IMPORT("next_event") int cb__next_event(char *out, int cap);

/* Calls any API function: obj is "machines", "energy", "computer", "hud", "world" or a machine id like "m5".
 * args_json is a JSON array, e.g. "[]" or "[\"iron_plate\"]". The JSON result is written to out.
 * Returns its length, 0 for null/false-ish nothing, or -1 on error (see cb_error()). */
static inline int cb_call(const char *obj, const char *fn, const char *args_json, char *out, int cap) {
    if (!args_json) args_json = "[]";
    int n = cb__call(obj, (int)strlen(obj), fn, (int)strlen(fn), args_json, (int)strlen(args_json), out, cap);
    if (n > 0 && n < cap) out[n] = 0; else if (cap > 0 && n <= 0) out[0] = 0;
    return n >= cap ? cap - 1 : n;
}

/* Last API error message (empty if none). */
static inline const char *cb_error(void) { static char e[256]; int n = cb__error(e, (int)sizeof e - 1); e[n > 0 ? n : 0] = 0; return e; }

/* Strips the quotes of a JSON string result in place ("iron_ore" -> iron_ore). */
static inline char *cb_unquote(char *s) {
    size_t n = strlen(s);
    if (n >= 2 && s[0] == '"' && s[n - 1] == '"') { memmove(s, s + 1, n - 2); s[n - 2] = 0; }
    return s;
}

/* A machine operation: cb_op("m5", "mine", "[]", out, cap). Strings come back without quotes.
 * Returns the result length, 0 when the result is null (e.g. nothing to mine), -1 on error. */
static inline int cb_op(const char *machine_id, const char *op, const char *args_json, char *out, int cap) {
    int n = cb_call(machine_id, op, args_json, out, cap);
    if (n > 0 && strcmp(out, "null") == 0) { out[0] = 0; return 0; }
    if (n > 0) { cb_unquote(out); return (int)strlen(out) > 0 ? (int)strlen(out) : 1; }
    return n;
}

/* Finds the nearest machine of a type ("drill", "furnace", ...) and writes its id. Returns 1 if found. */
static inline int cb_find(const char *type, char *id, int cap) {
    char args[96], out[256];
    snprintf(args, sizeof args, "[\"%s\"]", type);
    if (cb_call("machines", "find", args, out, (int)sizeof out) <= 0 || strcmp(out, "null") == 0) return 0;
    const char *p = strstr(out, "\"id\":\"");
    if (!p) return 0;
    p += 6;
    int i = 0;
    while (*p && *p != '"' && i < cap - 1) id[i++] = *p++;
    id[i] = 0;
    return 1;
}

/* Reads a number field from JSON text: cb_num(status, "progress"). Returns 0 if missing. */
static inline double cb_num(const char *json, const char *key) {
    char pat[80]; snprintf(pat, sizeof pat, "\"%s\":", key);
    const char *p = strstr(json, pat);
    return p ? strtod(p + strlen(pat), NULL) : 0.0;
}

/* Reads a string field from JSON text into out. Returns 1 if found. */
static inline int cb_str(const char *json, const char *key, char *out, int cap) {
    char pat[80]; snprintf(pat, sizeof pat, "\"%s\":\"", key);
    const char *p = strstr(json, pat);
    if (!p || cap <= 0) return 0;
    p += strlen(pat);
    int i = 0;
    while (*p && *p != '"' && i < cap - 1) out[i++] = *p++;
    out[i] = 0;
    return 1;
}

/* Waits in game time (does not use CPU). */
static inline void cb_sleep(double seconds) { char a[48], o[16]; snprintf(a, sizeof a, "[%.6f]", seconds); cb_call("computer", "sleep", a, o, (int)sizeof o); }

/* Events: subscribe, then cb_wait_event() blocks until the next one and writes {"name":...,"data":...}. */
static inline void cb_subscribe(const char *event) { char a[96], o[16]; snprintf(a, sizeof a, "[\"%s\"]", event); cb_call("computer", "subscribe", a, o, (int)sizeof o); }
static inline int cb_wait_event(char *out, int cap) {
    int n = cb__next_event(out, cap);
    if (n > 0) { out[n < cap ? n : cap - 1] = 0; return n; }
    char tmp[16]; cb_call("computer", "wait_event", "[]", tmp, (int)sizeof tmp);
    n = cb__next_event(out, cap);
    if (n > 0) out[n < cap ? n : cap - 1] = 0;
    return n;
}

/* Logic network: send JSON data on a channel to every other computer listening to the "message" event. */
static inline void cb_send(const char *channel, const char *data_json) { char a[4200], o[16]; snprintf(a, sizeof a, "[\"%s\",%s]", channel, data_json ? data_json : "null"); cb_call("computer", "send", a, o, (int)sizeof o); }

/* HUD helpers */
static inline void cb_hud_set(const char *key, const char *value) { char a[512], o[16]; snprintf(a, sizeof a, "[\"%s\",\"%s\"]", key, value); cb_call("hud", "set", a, o, (int)sizeof o); }
static inline void cb_notify(const char *text) { char a[512], o[16]; snprintf(a, sizeof a, "[\"%s\"]", text); cb_call("hud", "notify", a, o, (int)sizeof o); }

#ifdef __cplusplus
}

#include <string>
namespace cb {
    /* C++ convenience: cb::Machine drill("drill"); while (!(s = drill.op("mine")).empty()) ... */
    struct Machine {
        std::string id;
        explicit Machine(const char *type) { char b[64]; if (cb_find(type, b, (int)sizeof b)) id = b; }
        bool found() const { return !id.empty(); }
        std::string op(const char *name, const char *args_json = "[]") const {
            static char out[65536];
            int n = cb_op(id.c_str(), name, args_json, out, (int)sizeof out);
            return n > 0 ? std::string(out) : std::string();
        }
    };
    inline std::string call(const char *obj, const char *fn, const char *args_json = "[]") {
        static char out[65536];
        return cb_call(obj, fn, args_json, out, (int)sizeof out) > 0 ? std::string(out) : std::string();
    }
    inline void sleep(double s) { cb_sleep(s); }
}
#endif
#endif
