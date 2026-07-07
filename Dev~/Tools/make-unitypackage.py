import os, io, tarfile, sys

# (source dir, target-path-prefix under Assets/)
PKG = r"c:/Users/jungs/00_LocalRepo/04_unity-modules"
MAP = [
    (PKG + "/Runtime",            "Assets/Border/Runtime"),
    (PKG + "/Editor",             "Assets/Border/Editor"),
    (PKG + "/Plugins~/borderjung","Assets/Border/Demo"),
]
OUT = sys.argv[1] if len(sys.argv) > 1 else "Border-Modules.unitypackage"

def read_guid(meta_path):
    with io.open(meta_path, encoding="utf-8") as f:
        for line in f:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    return None

entries = []  # (guid, asset_bytes_or_None, meta_bytes, pathname)
seen = set()
for src_root, dst_prefix in MAP:
    for root, dirs, files in os.walk(src_root):
        for fn in files:
            if fn.endswith(".meta"):
                continue
            fpath = os.path.join(root, fn)
            mpath = fpath + ".meta"
            if not os.path.exists(mpath):
                continue  # 메타 없는 파일은 스킵
            guid = read_guid(mpath)
            if not guid or guid in seen:
                continue
            seen.add(guid)
            rel = os.path.relpath(fpath, src_root).replace("\\", "/")
            pathname = dst_prefix + "/" + rel
            with open(fpath, "rb") as f: asset = f.read()
            with open(mpath, "rb") as f: meta = f.read()
            entries.append((guid, asset, meta, pathname))

# .unitypackage = gzip tar; 각 asset은 <guid>/{asset,asset.meta,pathname}
def add(tar, name, data):
    ti = tarfile.TarInfo(name)
    ti.size = len(data)
    ti.mode = 0o644
    tar.addfile(ti, io.BytesIO(data))

with tarfile.open(OUT, "w:gz") as tar:
    for guid, asset, meta, pathname in entries:
        add(tar, f"{guid}/asset", asset)
        add(tar, f"{guid}/asset.meta", meta)
        add(tar, f"{guid}/pathname", pathname.encode("utf-8"))

print(f"{OUT}: {len(entries)} assets 패키징")
print("경로 샘플:")
for e in entries[:3] + entries[-3:]:
    print("  ", e[3])
