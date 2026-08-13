"""
DriftLift Asset Pipeline
- Removes backgrounds from controller images using rembg
- Converts to PNG with transparency
- Copies final PNGs to the WPF Assets folder
"""
import sys
import os
from pathlib import Path

BRAIN_DIR = r"C:\Users\Parsian\.gemini\antigravity\brain\a6e6704a-7701-4d92-ab0b-0a99857a741f"
ASSETS_DIR = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets"

SOURCES = {
    "xbox_placeholder.png": os.path.join(BRAIN_DIR, "xbox_controller_flat_1785984930817.jpg"),
    "ps4_placeholder.png":  os.path.join(BRAIN_DIR, "ps4_controller_flat_1785984947667.jpg"),
}

def remove_bg(input_path: str, output_path: str):
    from rembg import remove
    from PIL import Image
    import io

    print(f"  Processing: {input_path}")
    with open(input_path, "rb") as f:
        data = f.read()

    result = remove(data)
    img = Image.open(io.BytesIO(result)).convert("RGBA")

    # Crop to tight bounding box (remove empty transparent margin)
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    # Add small padding so buttons aren't cut off at edges
    padded = Image.new("RGBA", (img.width + 20, img.height + 20), (0, 0, 0, 0))
    padded.paste(img, (10, 10))

    padded.save(output_path, "PNG")
    print(f"  ✓ Saved: {output_path}  ({padded.width}x{padded.height})")

def main():
    os.makedirs(ASSETS_DIR, exist_ok=True)
    
    for out_name, src_path in SOURCES.items():
        out_path = os.path.join(ASSETS_DIR, out_name)
        if not os.path.exists(src_path):
            print(f"  ✗ Source not found: {src_path}")
            continue
        try:
            remove_bg(src_path, out_path)
        except Exception as e:
            print(f"  ✗ Failed {out_name}: {e}")
            # Fallback: just copy and convert without removing bg
            try:
                from PIL import Image
                img = Image.open(src_path).convert("RGBA")
                img.save(out_path, "PNG")
                print(f"  ~ Fallback copy saved: {out_path}")
            except Exception as e2:
                print(f"  ✗ Fallback also failed: {e2}")

    print("\nDone! Check Assets folder.")

if __name__ == "__main__":
    main()
