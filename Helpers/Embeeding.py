from sentence_transformers import SentenceTransformer
import sys, json, traceback
import codecs

try:
    # Lokal model yolunu veriyoruz
    model = SentenceTransformer(
        "intfloat/e5-base-v2"
    )

    chunks_json = sys.stdin.read()
    
    if chunks_json.startswith('\ufeff'):
        chunks_json = chunks_json[1:]
    if chunks_json.startswith(codecs.BOM_UTF8.decode('utf-8')):
        chunks_json = chunks_json[len(codecs.BOM_UTF8.decode('utf-8')):]
    
    chunks_json = chunks_json.strip()
    
    if not chunks_json:
        print(json.dumps([]), flush=True)
        sys.exit(0)
    
    chunks = json.loads(chunks_json)

    embeddings = model.encode(chunks).tolist()
    print(json.dumps(embeddings), flush=True)

except Exception as e:
    print(json.dumps([]), flush=True)
