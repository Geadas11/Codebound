"""Reverse direct-call search (read-only): which functions contain `call rel32` to the given functions.
Usage: python -I callers.py <factorio.exe> <pdb_proc_ranges.tsv> "Func::name" [...]
shortcut: same byte-scan limits as calls.py (no indirect/virtual calls)."""
import bisect, mmap, struct, sys
exe, ranges, *names = sys.argv[1:]
rows = [l.rstrip("\n").split("\t") for l in open(ranges, encoding="utf-8")]
text = sorted((int(o), int(l), n) for n, s, o, l in rows if s == "1")
starts = [t[0] for t in text]
want = {}
for o, l, n in text:
    if n in names:
        want.setdefault(o, n)
fh = open(exe, "rb")  # keep the handle alive for mmap
m = mmap.mmap(fh.fileno(), 0, access=mmap.ACCESS_READ)
code = m[0x400:0x400 + text[-1][0] + text[-1][1]]
hits = {n: set() for n in names}
i = code.find(b"\xe8")
while i != -1 and i + 5 <= len(code):
    tgt = i + 5 + struct.unpack_from("<i", code, i + 1)[0]
    if tgt in want:
        k = bisect.bisect_right(starts, i) - 1
        if k >= 0 and i < text[k][0] + text[k][1]:
            hits[want[tgt]].add(text[k][2])
    i = code.find(b"\xe8", i + 1)
for n in names:
    print(f"### {n} <- " + (" | ".join(sorted(hits[n])) or "(no direct callers: indirect/virtual only)"))
