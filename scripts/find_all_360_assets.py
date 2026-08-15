import os
from PIL import Image

src_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images"

for root, dirs, files in os.walk(src_dir):
    for f in files:
        if f.endswith('.png'):
            p = os.path.join(root, f)
            im = Image.open(p)
            print(f"{os.path.relpath(p, src_dir):65s}: size={im.size}")
