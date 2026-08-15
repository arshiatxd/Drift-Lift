"""
Refined background removal for gamepadviewer style transparent PNG assets
"""
import os
from pathlib import Path
from rembg import remove
from PIL import Image
import io

BRAIN_DIR = r"C:\Users\Parsian\.gemini\antigravity\brain\a6e6704a-7701-4d92-ab0b-0a99857a741f"
ASSETS_DIR = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets"

SOURCES = {
    "ps4_placeholder.png": os.path.join(BRAIN_DIR, "gamepadviewer_ps4_transparent_1785985923915.jpg"),
    "xbox_placeholder.png": os.path.join(BRAIN_DIR, "gamepadviewer_xbox_transparent_1785985937433.jpg"),
}

def clean_and_save(src_file, dst_file):
    print(f"Processing {src_file}...")
    with open(src_file, "rb") as f:
        data = f.read()
    
    output_bytes = remove(data)
    img = Image.open(io.BytesIO(output_bytes)).convert("RGBA")
    
    # Trim empty margin
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
        
    # Resize to standardized aspect ratio for 420x260 canvas
    # Canvas target is 420x260
    # Let's pad it cleanly so it centers inside 420x260 proportion
    target_w, target_h = 840, 520
    img.thumbnail((target_w - 20, target_h - 20), Image.Resampling.LANCZOS)
    
    final_img = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    offset_x = (target_w - img.width) // 2
    offset_y = (target_h - img.height) // 2
    final_img.paste(img, (offset_x, offset_y), img)
    
    final_img.save(dst_file, "PNG")
    print(f"✓ Saved {dst_file} ({final_img.width}x{final_img.height})")

def main():
    os.makedirs(ASSETS_DIR, exist_ok=True)
    for name, src in SOURCES.items():
        dst = os.path.join(ASSETS_DIR, name)
        clean_and_save(src, dst)

if __name__ == "__main__":
    main()
