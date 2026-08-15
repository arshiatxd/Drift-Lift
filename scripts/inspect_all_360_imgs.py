import os
from PIL import Image

xb_root = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images"

for root, dirs, files in os.walk(xb_root):
    for f in files:
        if f.endswith('.png') and any(k in f.lower() for k in ['overlay', 'bumper', 'trigger', 'lb', 'rb', 'lt', 'rt']):
            p = os.path.join(root, f)
            im = Image.open(p)
            print(f"{os.path.relpath(p, xb_root):60s}: size={im.size}")
