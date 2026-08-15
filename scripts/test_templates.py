from PIL import Image
import os

p1 = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black (No Thumbstick).png"
p2 = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\XB360_base_black.png"

im1 = Image.open(p1)
im2 = Image.open(p2)

print(f"im1 (No Thumbstick): {im1.size}, mode={im1.mode}")
print(f"im2 (Base): {im2.size}, mode={im2.mode}")

# Let's save a preview to inspect
im1.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\preview_no_thumb.png")
