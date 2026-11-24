"""Read-only MSF 7.00 / PDB parser (stdlib only).

Usage: python -I pdb_dump.py <factorio.pdb> <out_dir>
Opens the PDB with mmap ACCESS_READ; never writes to the input.
Outputs (in out_dir):
  pdb_info.json          PDB/DBI headers, stream counts, build info
  pdb_modules.tsv        DBI module list: index, module name, obj/lib file, sym bytes
  pdb_source_files.txt   unique source files referenced by modules
  pdb_procs.txt          global procedure names (S_PROCREF/S_LPROCREF from globals stream)
  pdb_publics.tsv        public symbols: seg, offset, mangled name
  pdb_gdata.txt          global data names (S_GDATA32/S_LDATA32)
  pdb_udts.tsv           every named class/struct/union/enum: kind, size, name, fieldcount
  pdb_layouts.txt        member layouts (offset, type, name, bases, vfptr) of non-std classes
  pdb_udt_src.tsv        IPI LF_UDT_SRC_LINE: type name -> source file:line
"""
import json, mmap, os, struct, sys
from collections import Counter

MAGIC = b"Microsoft C/C++ MSF 7.00\r\n\x1aDS\x00\x00\x00"


class MSF:
    def __init__(self, path):
        self.f = open(path, "rb")
        self.m = mmap.mmap(self.f.fileno(), 0, access=mmap.ACCESS_READ)
        assert self.m[:32] == MAGIC, "not an MSF 7.00 PDB"
        self.bs, _fpm, self.nblocks, dirbytes, _u, bmaddr = struct.unpack_from("<6I", self.m, 32)
        ndirblocks = -(-dirbytes // self.bs)
        dblocks = struct.unpack_from(f"<{ndirblocks}I", self.m, bmaddr * self.bs)
        d = b"".join(self.m[b * self.bs:(b + 1) * self.bs] for b in dblocks)[:dirbytes]
        n = struct.unpack_from("<I", d, 0)[0]
        sizes = struct.unpack_from(f"<{n}I", d, 4)
        pos = 4 + 4 * n
        self.streams = []
        for s in sizes:
            if s == 0xFFFFFFFF:
                self.streams.append((0, ()))
                continue
            nb = -(-s // self.bs)
            self.streams.append((s, struct.unpack_from(f"<{nb}I", d, pos)))
            pos += 4 * nb

    def stream(self, i):
        size, blocks = self.streams[i]
        bs = self.bs
        return b"".join(self.m[b * bs:(b + 1) * bs] for b in blocks)[:size]


def cstr(b, p):
    e = b.index(b"\0", p)
    return b[p:e].decode("utf-8", "replace"), e + 1


NUMERIC = {0x8000: ("<b", 1), 0x8001: ("<h", 2), 0x8002: ("<H", 2), 0x8003: ("<i", 4),
           0x8004: ("<I", 4), 0x8009: ("<q", 8), 0x800A: ("<Q", 8)}


def numeric(b, p):
    v = struct.unpack_from("<H", b, p)[0]
    if v < 0x8000:
        return v, p + 2
    fmt, n = NUMERIC.get(v, (None, 0))
    if fmt is None:
        return 0, p + 2  # shortcut: exotic numeric leaves (float/decimal) read as 0; never seen in member offsets
    return struct.unpack_from(fmt, b, p + 2)[0], p + 2 + n


PRIM = {0x03: "void", 0x10: "int8", 0x20: "uint8", 0x11: "int16", 0x21: "uint16",
        0x12: "int32", 0x22: "uint32", 0x13: "int64", 0x23: "uint64", 0x30: "bool",
        0x40: "float", 0x41: "double", 0x68: "int8", 0x69: "uint8", 0x70: "char",
        0x71: "wchar_t", 0x72: "int16", 0x73: "uint16", 0x74: "int32", 0x75: "uint32",
        0x76: "int64", 0x77: "uint64", 0x7a: "char16_t", 0x7b: "char32_t", 0x7c: "char8_t",
        0x31: "bool16", 0x32: "bool32", 0x42: "long double", 0x08: "HRESULT"}


class TPI:
    def __init__(self, data):
        self.data = data
        _ver, hsz, self.begin, self.end, _nbytes = struct.unpack_from("<5I", data, 0)
        self.off = []
        p = hsz
        while p < len(data):
            ln = struct.unpack_from("<H", data, p)[0]
            self.off.append(p)
            p += 2 + ln
        self.cache = {}

    def rec(self, ti):
        p = self.off[ti - self.begin]
        ln, kind = struct.unpack_from("<HH", self.data, p)
        return kind, self.data[p + 4:p + 2 + ln]

    def name(self, ti, depth=0):
        if ti < 0x1000:
            base = PRIM.get(ti & 0xFF, f"prim_{ti:#x}")
            return base + ("*" if ti & 0x700 else "")
        if ti in self.cache:
            return self.cache[ti]
        if depth > 12 or ti - self.begin >= len(self.off):
            return f"T{ti:#x}"
        kind, b = self.rec(ti)
        r = f"T{ti:#x}"
        try:
            if kind in (0x1504, 0x1505):
                _c, _pr, _fl, _d, _vs = struct.unpack_from("<HHIII", b, 0)
                _, p = numeric(b, 16)
                r = cstr(b, p)[0]
            elif kind == 0x1506:
                _, p = numeric(b, 8)
                r = cstr(b, p)[0]
            elif kind == 0x1507:
                r = cstr(b, 12)[0]
            elif kind == 0x1002:
                ut, attr = struct.unpack_from("<II", b, 0)
                mode = (attr >> 5) & 7
                r = self.name(ut, depth + 1) + ("&" if mode == 1 else "&&" if mode == 4 else "*")
            elif kind == 0x1001:
                ut, mods = struct.unpack_from("<IH", b, 0)
                r = ("const " if mods & 1 else "") + ("volatile " if mods & 2 else "") + self.name(ut, depth + 1)
            elif kind == 0x1503:
                el, _ix = struct.unpack_from("<II", b, 0)
                sz, _ = numeric(b, 8)
                r = f"{self.name(el, depth + 1)}[{sz}B]"
            elif kind == 0x1205:
                ut = struct.unpack_from("<I", b, 0)[0]
                r = self.name(ut, depth + 1) + ":bits"
            elif kind in (0x1008, 0x1009):
                r = "fn"
        except Exception:
            pass
        self.cache[ti] = r
        return r

    def fieldlist(self, ti):
        kind, b = self.rec(ti)
        out = []
        p = 0
        while p < len(b):
            if b[p] >= 0xF0:
                p += b[p] & 0x0F
                continue
            k = struct.unpack_from("<H", b, p)[0]
            q = p + 2
            if k == 0x150D:  # LF_MEMBER
                _a, t = struct.unpack_from("<HI", b, q)
                off, q = numeric(b, q + 6)
                nm, q = cstr(b, q)
                out.append(("member", off, self.name(t), nm))
            elif k == 0x1400:  # LF_BCLASS
                _a, t = struct.unpack_from("<HI", b, q)
                off, q = numeric(b, q + 6)
                out.append(("base", off, self.name(t), ""))
            elif k in (0x1401, 0x1402):  # LF_VBCLASS / LF_IVBCLASS
                _a, t, _vt = struct.unpack_from("<HII", b, q)
                _, q = numeric(b, q + 10)
                _, q = numeric(b, q)
                out.append(("vbase", 0, self.name(t), ""))
            elif k == 0x1409:  # LF_VFUNCTAB
                q += 6
                out.append(("vfptr", 0, "", ""))
            elif k == 0x150E:  # LF_STMEMBER
                _a, t = struct.unpack_from("<HI", b, q)
                nm, q = cstr(b, q + 6)
                out.append(("static", 0, self.name(t), nm))
            elif k == 0x1511:  # LF_ONEMETHOD
                a, _t = struct.unpack_from("<HI", b, q)
                q += 6
                if ((a >> 2) & 7) in (4, 6):
                    q += 4
                nm, q = cstr(b, q)
                out.append(("method", 0, "", nm))
            elif k == 0x150F:  # LF_METHOD
                cnt, _ml = struct.unpack_from("<HI", b, q)
                nm, q = cstr(b, q + 6)
                out.append(("method", cnt, "", nm))
            elif k == 0x1510:  # LF_NESTTYPE
                _pad, t = struct.unpack_from("<HI", b, q)
                nm, q = cstr(b, q + 6)
                out.append(("nested", 0, "", nm))
            elif k == 0x1502:  # LF_ENUMERATE
                _a = struct.unpack_from("<H", b, q)[0]
                v, q = numeric(b, q + 2)
                nm, q = cstr(b, q)
                out.append(("enum", v, "", nm))
            elif k == 0x1404:  # LF_INDEX continuation
                _pad, t = struct.unpack_from("<HI", b, q)
                q += 6
                out.extend(self.fieldlist(t))
            else:
                break  # unknown sub-leaf: stop rather than mis-parse
            p = q
        return out


SKIP_NS = ("std::", "boost::", "__", "<", "Concurrency::", "fmt::", "tbb::", "absl::", "_", "ATL::")


def main():
    pdb, out = sys.argv[1], sys.argv[2]
    os.makedirs(out, exist_ok=True)
    msf = MSF(pdb)
    info = {"block_size": msf.bs, "num_blocks": msf.nblocks, "num_streams": len(msf.streams)}

    # PDB info stream (1)
    s1 = msf.stream(1)
    ver, sig, age = struct.unpack_from("<III", s1, 0)
    info["pdb_info"] = {"version": ver, "signature_time": sig, "age": age, "guid": s1[12:28].hex()}
    # named stream map: string buffer, then hash table (size, capacity, present bitset, deleted bitset, pairs)
    bufsz = struct.unpack_from("<I", s1, 28)[0]
    nbuf = s1[32:32 + bufsz]
    q = 32 + bufsz
    size, _cap, nwords = struct.unpack_from("<III", s1, q)
    q += 12 + 4 * nwords
    q += 4 + 4 * struct.unpack_from("<I", s1, q)[0]
    named = {}
    for i in range(size):
        off, idx = struct.unpack_from("<II", s1, q + 8 * i)
        named[nbuf[off:nbuf.index(b"\0", off)].decode()] = idx
    ns = msf.stream(named["/names"])
    assert struct.unpack_from("<I", ns, 0)[0] == 0xEFFEEFFE, "bad /names header"
    names_buf = ns[12:12 + struct.unpack_from("<I", ns, 8)[0]]

    def names_str(off):
        return names_buf[off:names_buf.index(b"\0", off)].decode("utf-8", "replace")

    # DBI (3)
    dbi = msf.stream(3)
    (vsig, vhdr, dage, gsi, build, psi, dllver, symrec, _rb, modsz, sczsz, smsz, srcsz,
     tsmsz, _mfc, dbgsz, ecsz, flags, machine, _pad) = struct.unpack_from("<iIIHHHHHHiiiiiIiiHHI", dbi, 0)
    info["dbi"] = {"version": vhdr, "age": dage, "toolchain_major": (build >> 8) & 0x7F, "toolchain_minor": build & 0xFF,
                   "pdb_dll_version": dllver, "flags": flags, "machine": hex(machine),
                   "globals_stream": gsi, "publics_stream": psi, "symrec_stream": symrec}
    p = 64
    mods = []
    end = 64 + modsz
    while p < end:
        symstream, symbytes = struct.unpack_from("<HI", dbi, p + 34)
        srccount = struct.unpack_from("<H", dbi, p + 48)[0]
        nm, q = cstr(dbi, p + 64)
        obj, q = cstr(dbi, q)
        mods.append((nm, obj, symstream, symbytes, srccount))
        p = (q + 3) & ~3
    assert any(m[4] for m in mods), "ModInfo SourceFileCount offset wrong"
    info["module_count"] = len(mods)
    with open(os.path.join(out, "pdb_modules.tsv"), "w", encoding="utf-8") as f:
        f.write("index\tmodule\tobj_or_lib\tsym_stream\tsym_bytes\tsource_files\n")
        for i, m in enumerate(mods):
            f.write(f"{i}\t{m[0]}\t{m[1]}\t{m[2]}\t{m[3]}\t{m[4]}\n")

    # Source info substream
    sp = 64 + modsz + sczsz + smsz
    nmod, _nsrc = struct.unpack_from("<HH", dbi, sp)
    counts = struct.unpack_from(f"<{nmod}H", dbi, sp + 4 + 2 * nmod)
    total = sum(counts)
    offs = struct.unpack_from(f"<{total}I", dbi, sp + 4 + 4 * nmod)
    names_base = sp + 4 + 4 * nmod + 4 * total
    srcs = set()
    for o in set(offs):
        srcs.add(cstr(dbi, names_base + o)[0])
    with open(os.path.join(out, "pdb_source_files.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(sorted(srcs)))
    info["unique_source_files"] = len(srcs)

    # Symbol record stream: globals (procrefs, data, udt names)
    sym = msf.stream(symrec)
    procs, gdata, pubs = [], [], []
    kinds = Counter()
    p = 0
    while p + 4 <= len(sym):
        ln, k = struct.unpack_from("<HH", sym, p)
        b = sym[p + 4:p + 2 + ln]
        kinds[k] += 1
        try:
            if k in (0x1125, 0x1127):
                procs.append(cstr(b, 10)[0])
            elif k in (0x110C, 0x110D):
                gdata.append(cstr(b, 10)[0])
            elif k == 0x110E:
                _fl, off, seg = struct.unpack_from("<IIH", b, 0)
                pubs.append((seg, off, cstr(b, 10)[0]))
        except ValueError:
            pass
        p += 2 + ln
    info["symrec_kinds"] = {hex(k): v for k, v in kinds.most_common()}
    with open(os.path.join(out, "pdb_procs.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(sorted(set(procs))))
    with open(os.path.join(out, "pdb_gdata.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(sorted(set(gdata))))
    with open(os.path.join(out, "pdb_publics.tsv"), "w", encoding="utf-8") as f:
        f.write("seg\toffset\tname\n")
        for s, o, n in sorted(pubs):
            f.write(f"{s}\t{o:#x}\t{n}\n")
    info["procedures"] = len(set(procs))
    info["publics"] = len(pubs)
    info["global_data"] = len(set(gdata))

    # TPI (2)
    tpi = TPI(msf.stream(2))
    info["tpi"] = {"begin": hex(tpi.begin), "end": hex(tpi.end), "records": len(tpi.off)}
    udts = {}
    for i in range(len(tpi.off)):
        ti = tpi.begin + i
        kind, b = tpi.rec(ti)
        if kind not in (0x1504, 0x1505, 0x1506, 0x1507):
            continue
        try:
            if kind == 0x1507:
                cnt, prop, _ut, fl = struct.unpack_from("<HHII", b, 0)
                size, nm = 0, cstr(b, 12)[0]
            elif kind == 0x1506:
                cnt, prop, fl = struct.unpack_from("<HHI", b, 0)
                size, p = numeric(b, 8)
                nm = cstr(b, p)[0]
            else:
                cnt, prop, fl, _d, _v = struct.unpack_from("<HHIII", b, 0)
                size, p = numeric(b, 16)
                nm = cstr(b, p)[0]
        except Exception:
            continue
        if prop & 0x80:  # forward reference
            continue
        k = {0x1504: "class", 0x1505: "struct", 0x1506: "union", 0x1507: "enum"}[kind]
        udts.setdefault(nm, (k, size, cnt, fl, ti))
    with open(os.path.join(out, "pdb_udts.tsv"), "w", encoding="utf-8") as f:
        f.write("kind\tsize\tfields\tname\n")
        for nm in sorted(udts):
            k, size, cnt, _fl, _ti = udts[nm]
            f.write(f"{k}\t{size}\t{cnt}\t{nm}\n")
    info["udt_definitions"] = len(udts)
    nl = 0
    with open(os.path.join(out, "pdb_layouts.txt"), "w", encoding="utf-8") as f:
        for nm in sorted(udts):
            k, size, cnt, fl, _ti = udts[nm]
            if nm.startswith(SKIP_NS) or "<lambda" in nm or "`" in nm:
                continue
            try:
                fields = tpi.fieldlist(fl) if fl else []
            except Exception as e:
                fields = [("error", 0, "", str(e))]
            f.write(f"== {k} {nm} size={size}\n")
            for fk, off, ty, fn in fields:
                if fk == "method":
                    continue
                if fk == "enum":
                    f.write(f"   {fn} = {off}\n")
                else:
                    f.write(f"   {fk:6} +{off:<6} {ty} {fn}\n")
            meths = sorted({fn for fk, _o, _t, fn in fields if fk == "method"})
            if meths:
                f.write(f"   methods: {', '.join(meths)}\n")
            nl += 1
    info["layouts_written"] = nl

    # IPI (4): LF_UDT_SRC_LINE (0x1606) / LF_UDT_MOD_SRC_LINE (0x1607), LF_STRING_ID (0x1605), LF_BUILDINFO (0x1603)
    try:
        ipi = TPI(msf.stream(4))
        strings = {}
        srclines = []
        build = []
        for i in range(len(ipi.off)):
            ti = ipi.begin + i
            kind, b = ipi.rec(ti)
            if kind == 0x1605:
                strings[ti] = cstr(b, 4)[0]
            elif kind in (0x1606, 0x1607):
                t, sid, line = struct.unpack_from("<III", b, 0)
                srclines.append((t, sid, line, kind))
            elif kind == 0x1603 and len(build) < 3:
                n = struct.unpack_from("<H", b, 0)[0]
                build.append(struct.unpack_from(f"<{n}I", b, 2))
        with open(os.path.join(out, "pdb_udt_src.tsv"), "w", encoding="utf-8") as f:
            f.write("type\tsource\tline\n")
            for t, sid, line, kind in srclines:
                src = strings.get(sid, f"?{sid}") if kind == 0x1606 else names_str(sid)
                f.write(f"{tpi.name(t)}\t{src}\t{line}\n")
        info["ipi_udt_src_lines"] = len(srclines)
        info["buildinfo_samples"] = [[strings.get(x, hex(x)) for x in bi] for bi in build]
    except Exception as e:
        info["ipi_error"] = repr(e)

    with open(os.path.join(out, "pdb_info.json"), "w", encoding="utf-8") as f:
        json.dump(info, f, indent=1)
    print(json.dumps({k: v for k, v in info.items() if k != "symrec_kinds"}, indent=1)[:4000])


if __name__ == "__main__":
    main()
