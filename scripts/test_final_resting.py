from PIL import Image
import os

base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black (No Thumbstick).png"
press_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"

base = Image.open(base_path).convert("RGBA")
scale = 260.0 / 955.0
dx = (420.0 - 1545.0 * scale) / 2.0
dy = 0.0

scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

# Test items (resting state + stick caps):
items = [
    # Resting stick caps:
    ("XB360_LeftStick_Black.png", 58.1, 111.3, 49.0, 49.0),
    ("XB360_RightStick_Black.png", 245.9, 172.2, 49.0, 49.0),
]

for fname, cx, cy, cw, ch in items:
    p = os.path.join(press_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_final_resting.png")
print("Saved test_final_resting.png")
