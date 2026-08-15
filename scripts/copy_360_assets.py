import os
import shutil
from PIL import Image

src_360_base = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\XB360_base_black.png"
src_360_press = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Theme Assets\Active Presses"
src_360_color = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Theme Assets\Active Presses\Button Color"

dst_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"
os.makedirs(dst_dir, exist_ok=True)

# Copy base
shutil.copy2(src_360_base, os.path.join(dst_dir, "XB360_Base.png"))
print("Copied XB360_Base.png")

# Copy active presses
for f in os.listdir(src_360_press):
    if f.endswith('.png'):
        shutil.copy2(os.path.join(src_360_press, f), os.path.join(dst_dir, f))
        print(f"Copied {f}")

# Copy stick colors if exist
for f in os.listdir(src_360_color):
    if f.endswith('.png'):
        shutil.copy2(os.path.join(src_360_color, f), os.path.join(dst_dir, f))
        print(f"Copied color/{f}")
